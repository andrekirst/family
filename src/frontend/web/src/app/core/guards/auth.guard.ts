import { Injectable, inject } from '@angular/core';
import { CanActivate, CanActivateChild, Router } from '@angular/router';
import { Observable, map, tap } from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild {
  
  private authService = inject(AuthService);
  private router = inject(Router);


  canActivate(): Observable<boolean> {
    return this.checkAuth();
  }

  canActivateChild(): Observable<boolean> {
    return this.checkAuth();
  }

  private checkAuth(): Observable<boolean> {
    return this.authService.isAuthenticated$.pipe(
      tap(isAuthenticated => {
        if (!isAuthenticated) {
          this.router.navigate(['/auth/login']);
        }
      }),
      map(isAuthenticated => isAuthenticated)
    );
  }
}