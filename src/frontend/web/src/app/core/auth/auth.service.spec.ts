import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Apollo } from 'apollo-angular';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { User, LoginInput } from '../graphql/types';

describe('AuthService', () => {
  let service: AuthService;
  let apollo: jasmine.SpyObj<Apollo>;
  let router: jasmine.SpyObj<Router>;

  const mockUser: User = {
    id: '1',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    fullName: 'Test User',
    preferredLanguage: 'de',
    isActive: true,
    createdAt: '2023-01-01T00:00:00Z',
    lastLoginAt: '2023-01-01T00:00:00Z'
  };

  beforeEach(() => {
    const apolloSpy = jasmine.createSpyObj('Apollo', ['mutate', 'query']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: Apollo, useValue: apolloSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(AuthService);
    apollo = TestBed.inject(Apollo) as jasmine.SpyObj<Apollo>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Mock localStorage
    spyOn(localStorage, 'getItem').and.returnValue(null);
    spyOn(localStorage, 'setItem');
    spyOn(localStorage, 'removeItem');
    spyOn(sessionStorage, 'getItem').and.returnValue(null);
    spyOn(sessionStorage, 'setItem');
    spyOn(sessionStorage, 'removeItem');
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('directLogin', () => {
    it('should handle successful login', (done) => {
      const loginInput: LoginInput = { email: 'test@example.com', password: 'password' };
      const mockResponse = {
        data: {
          directLogin: {
            accessToken: 'test-token',
            refreshToken: 'refresh-token',
            user: mockUser,
            errors: null
          }
        }
      };

      apollo.mutate.and.returnValue(of(mockResponse));

      service.directLogin(loginInput).subscribe(result => {
        expect(result.success).toBe(true);
        expect(localStorage.setItem).toHaveBeenCalledWith('accessToken', 'test-token');
        expect(localStorage.setItem).toHaveBeenCalledWith('refreshToken', 'refresh-token');
        done();
      });
    });

    it('should handle login failure', (done) => {
      const loginInput: LoginInput = { email: 'test@example.com', password: 'wrong-password' };
      const mockResponse = {
        data: {
          directLogin: {
            accessToken: null,
            refreshToken: null,
            user: null,
            errors: ['Invalid credentials']
          }
        }
      };

      apollo.mutate.and.returnValue(of(mockResponse));

      service.directLogin(loginInput).subscribe(result => {
        expect(result.success).toBe(false);
        expect(result.errors).toEqual(['Invalid credentials']);
        done();
      });
    });

    it('should handle network error', (done) => {
      const loginInput: LoginInput = { email: 'test@example.com', password: 'password' };
      apollo.mutate.and.returnValue(throwError(() => new Error('Network error')));

      service.directLogin(loginInput).subscribe(result => {
        expect(result.success).toBe(false);
        expect(result.errors).toEqual(['Network error occurred']);
        done();
      });
    });
  });

  describe('initiateOAuthLogin', () => {
    it('should handle successful OAuth initiation', (done) => {
      const mockResponse = {
        data: {
          initiateLogin: {
            loginUrl: 'https://keycloak.example.com/auth',
            state: 'test-state'
          }
        }
      };

      apollo.mutate.and.returnValue(of(mockResponse));

      service.initiateOAuthLogin().subscribe(result => {
        expect(result.loginUrl).toBe('https://keycloak.example.com/auth');
        expect(result.state).toBe('test-state');
        expect(sessionStorage.setItem).toHaveBeenCalledWith('oauth_state', 'test-state');
        done();
      });
    });
  });

  describe('logout', () => {
    it('should handle successful logout', (done) => {
      (localStorage.getItem as jasmine.Spy).and.returnValue('test-token');
      const mockResponse = {
        data: {
          logout: {
            success: true,
            errors: null
          }
        }
      };

      apollo.mutate.and.returnValue(of(mockResponse));

      service.logout().subscribe(result => {
        expect(result.success).toBe(true);
        expect(localStorage.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorage.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
        done();
      });
    });

    it('should handle logout without token', (done) => {
      (localStorage.getItem as jasmine.Spy).and.returnValue(null);

      service.logout().subscribe(result => {
        expect(result.success).toBe(true);
        expect(localStorage.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorage.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
        done();
      });
    });
  });

  describe('token management', () => {
    it('should get access token from localStorage', () => {
      (localStorage.getItem as jasmine.Spy).and.returnValue('test-token');
      
      const token = service.getAccessToken();
      
      expect(token).toBe('test-token');
      expect(localStorage.getItem).toHaveBeenCalledWith('accessToken');
    });

    it('should get refresh token from localStorage', () => {
      (localStorage.getItem as jasmine.Spy).and.returnValue('refresh-token');
      
      const token = service.getRefreshToken();
      
      expect(token).toBe('refresh-token');
      expect(localStorage.getItem).toHaveBeenCalledWith('refreshToken');
    });
  });

  describe('observables', () => {
    it('should emit authentication state changes', (done) => {
      service.isAuthenticated$.subscribe(isAuthenticated => {
        expect(typeof isAuthenticated).toBe('boolean');
        done();
      });
    });

    it('should emit current user changes', (done) => {
      service.currentUser$.subscribe(user => {
        expect(user).toBeNull(); // Initially null
        done();
      });
    });

    it('should emit loading state changes', (done) => {
      service.isLoading$.subscribe(isLoading => {
        expect(typeof isLoading).toBe('boolean');
        done();
      });
    });
  });
});