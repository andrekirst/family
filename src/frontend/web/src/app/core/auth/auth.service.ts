import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { BehaviorSubject, Observable, map, tap, catchError, of } from 'rxjs';
import { Router } from '@angular/router';
import { 
  DIRECT_LOGIN, 
  INITIATE_LOGIN, 
  COMPLETE_LOGIN, 
  REFRESH_TOKEN, 
  LOGOUT, 
  GET_CURRENT_USER 
} from '../graphql/queries';
import {
  User,
  LoginInput,
  LoginCallbackInput,
  RefreshTokenInput,
  DirectLoginResponse,
  InitiateLoginResponse,
  CompleteLoginResponse,
  RefreshTokenResponse,
  LogoutResponse,
  GetCurrentUserResponse
} from '../graphql/types';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private isLoadingSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();

  private apollo = inject(Apollo);
  private router = inject(Router);

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = this.getAccessToken();
    if (token) {
      this.loadCurrentUser().subscribe();
    }
  }

  directLogin(loginInput: LoginInput): Observable<{ success: boolean; errors?: string[] }> {
    this.isLoadingSubject.next(true);
    
    return this.apollo.mutate<DirectLoginResponse>({
      mutation: DIRECT_LOGIN,
      variables: { input: loginInput }
    }).pipe(
      map(result => {
        const loginPayload = result.data?.directLogin;
        if (loginPayload?.accessToken && loginPayload?.user) {
          this.handleSuccessfulLogin(
            loginPayload.accessToken,
            loginPayload.refreshToken,
            loginPayload.user
          );
          return { success: true };
        } else {
          return { 
            success: false, 
            errors: loginPayload?.errors || ['Login failed'] 
          };
        }
      }),
      catchError(error => {
        console.error('Direct login error:', error);
        return of({ success: false, errors: ['Network error occurred'] });
      }),
      tap(() => this.isLoadingSubject.next(false))
    );
  }

  initiateOAuthLogin(): Observable<{ loginUrl?: string; state?: string; errors?: string[] }> {
    this.isLoadingSubject.next(true);
    
    return this.apollo.mutate<InitiateLoginResponse>({
      mutation: INITIATE_LOGIN
    }).pipe(
      map(result => {
        const payload = result.data?.initiateLogin;
        if (payload?.loginUrl && payload?.state) {
          // Store state for OAuth callback validation
          sessionStorage.setItem('oauth_state', payload.state);
          return { loginUrl: payload.loginUrl, state: payload.state };
        } else {
          return { errors: ['Failed to initiate OAuth login'] };
        }
      }),
      catchError(error => {
        console.error('OAuth initiation error:', error);
        return of({ errors: ['Network error occurred'] });
      }),
      tap(() => this.isLoadingSubject.next(false))
    );
  }

  completeOAuthLogin(authorizationCode: string, state: string): Observable<{ success: boolean; errors?: string[] }> {
    this.isLoadingSubject.next(true);
    
    // Validate state parameter
    const storedState = sessionStorage.getItem('oauth_state');
    if (storedState !== state) {
      this.isLoadingSubject.next(false);
      return of({ success: false, errors: ['Invalid state parameter'] });
    }

    const input: LoginCallbackInput = { authorizationCode, state };
    
    return this.apollo.mutate<CompleteLoginResponse>({
      mutation: COMPLETE_LOGIN,
      variables: { input }
    }).pipe(
      map(result => {
        const loginPayload = result.data?.completeLogin;
        if (loginPayload?.accessToken && loginPayload?.user) {
          this.handleSuccessfulLogin(
            loginPayload.accessToken,
            loginPayload.refreshToken,
            loginPayload.user
          );
          // Clean up OAuth state
          sessionStorage.removeItem('oauth_state');
          return { success: true };
        } else {
          return { 
            success: false, 
            errors: loginPayload?.errors || ['OAuth login failed'] 
          };
        }
      }),
      catchError(error => {
        console.error('OAuth completion error:', error);
        return of({ success: false, errors: ['Network error occurred'] });
      }),
      tap(() => this.isLoadingSubject.next(false))
    );
  }

  refreshAccessToken(): Observable<{ success: boolean; errors?: string[] }> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of({ success: false, errors: ['No refresh token available'] });
    }

    const input: RefreshTokenInput = { refreshToken };
    
    return this.apollo.mutate<RefreshTokenResponse>({
      mutation: REFRESH_TOKEN,
      variables: { input }
    }).pipe(
      map(result => {
        const payload = result.data?.refreshToken;
        if (payload?.accessToken) {
          this.setAccessToken(payload.accessToken);
          if (payload.refreshToken) {
            this.setRefreshToken(payload.refreshToken);
          }
          return { success: true };
        } else {
          this.logout();
          return { 
            success: false, 
            errors: payload?.errors || ['Token refresh failed'] 
          };
        }
      }),
      catchError(error => {
        console.error('Token refresh error:', error);
        this.logout();
        return of({ success: false, errors: ['Network error occurred'] });
      })
    );
  }

  logout(): Observable<{ success: boolean; errors?: string[] }> {
    const accessToken = this.getAccessToken();
    
    if (accessToken) {
      return this.apollo.mutate<LogoutResponse>({
        mutation: LOGOUT,
        variables: { accessToken }
      }).pipe(
        map(result => {
          const payload = result.data?.logout;
          this.handleLogout();
          return { 
            success: payload?.success || true, 
            errors: payload?.errors 
          };
        }),
        catchError(error => {
          console.error('Logout error:', error);
          this.handleLogout();
          return of({ success: true });
        })
      );
    } else {
      this.handleLogout();
      return of({ success: true });
    }
  }

  loadCurrentUser(): Observable<User | null> {
    const token = this.getAccessToken();
    if (!token) {
      return of(null);
    }

    return this.apollo.query<GetCurrentUserResponse>({
      query: GET_CURRENT_USER,
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => {
        const user = result.data?.currentUser;
        if (user) {
          this.currentUserSubject.next(user);
          this.isAuthenticatedSubject.next(true);
          return user;
        } else {
          this.handleLogout();
          return null;
        }
      }),
      catchError(error => {
        console.error('Load current user error:', error);
        this.handleLogout();
        return of(null);
      })
    );
  }

  private handleSuccessfulLogin(accessToken: string, refreshToken: string | undefined, user: User): void {
    this.setAccessToken(accessToken);
    if (refreshToken) {
      this.setRefreshToken(refreshToken);
    }
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private handleLogout(): void {
    this.clearTokens();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    sessionStorage.removeItem('oauth_state');
    this.router.navigate(['/auth/login']);
  }

  // Token management methods
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  private setAccessToken(token: string): void {
    localStorage.setItem('accessToken', token);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  private setRefreshToken(token: string): void {
    localStorage.setItem('refreshToken', token);
  }

  private clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  // Utility methods
  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  get isLoading(): boolean {
    return this.isLoadingSubject.value;
  }
}