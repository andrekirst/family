import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatIconModule,
    MatSnackBarModule
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>Family Login</mat-card-title>
          <mat-card-subtitle>Melden Sie sich in Ihrem Family-Konto an</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <!-- OAuth Login Button -->
          <button 
            mat-raised-button 
            color="primary" 
            class="oauth-button"
            (click)="loginWithOAuth()"
            [disabled]="(isLoading$ | async) || false">
            <mat-icon>login</mat-icon>
            Mit Keycloak anmelden
          </button>
          
          <mat-divider class="divider">
            <span class="divider-text">oder</span>
          </mat-divider>
          
          <!-- Direct Login Form -->
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>E-Mail</mat-label>
              <input 
                matInput 
                type="email" 
                formControlName="email"
                placeholder="ihre@email.com"
                autocomplete="email">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
                E-Mail ist erforderlich
              </mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
                Bitte geben Sie eine gültige E-Mail ein
              </mat-error>
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Passwort</mat-label>
              <input 
                matInput 
                [type]="hidePassword ? 'password' : 'text'" 
                formControlName="password"
                autocomplete="current-password">
              <button 
                mat-icon-button 
                matSuffix 
                type="button"
                (click)="hidePassword = !hidePassword"
                [attr.aria-label]="'Hide password'"
                [attr.aria-pressed]="hidePassword">
                <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
                Passwort ist erforderlich
              </mat-error>
            </mat-form-field>
            
            <button 
              mat-raised-button 
              color="accent" 
              type="submit"
              class="full-width submit-button"
              [disabled]="loginForm.invalid || (isLoading$ | async) || false">
              <mat-spinner diameter="20" *ngIf="isLoading$ | async"></mat-spinner>
              <span *ngIf="!(isLoading$ | async)">Anmelden</span>
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }
    
    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 20px;
    }
    
    .oauth-button {
      width: 100%;
      height: 48px;
      margin-bottom: 20px;
      font-size: 16px;
    }
    
    .divider {
      margin: 20px 0;
      position: relative;
    }
    
    .divider-text {
      background: white;
      padding: 0 16px;
      color: rgba(0, 0, 0, 0.6);
      position: absolute;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
    }
    
    .login-form {
      margin-top: 20px;
    }
    
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    
    .submit-button {
      height: 48px;
      font-size: 16px;
      margin-top: 8px;
    }
    
    mat-card-header {
      margin-bottom: 20px;
    }
    
    mat-card-title {
      font-size: 24px;
      font-weight: 500;
    }
    
    mat-card-subtitle {
      font-size: 14px;
      color: rgba(0, 0, 0, 0.6);
      margin-top: 8px;
    }
  `]
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading$: Observable<boolean>;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
    
    this.isLoading$ = this.authService.isLoading$;
  }

  ngOnInit(): void {
    // Check if user is already authenticated
    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      
      this.authService.directLogin({ email, password }).subscribe({
        next: (result) => {
          if (result.success) {
            this.router.navigate(['/dashboard']);
            this.snackBar.open('Erfolgreich angemeldet!', 'Schließen', {
              duration: 3000
            });
          } else {
            this.showErrors(result.errors || ['Anmeldung fehlgeschlagen']);
          }
        },
        error: (error) => {
          console.error('Login error:', error);
          this.snackBar.open('Ein Fehler ist aufgetreten. Bitte versuchen Sie es erneut.', 'Schließen', {
            duration: 5000
          });
        }
      });
    }
  }

  loginWithOAuth(): void {
    this.authService.initiateOAuthLogin().subscribe({
      next: (result) => {
        if (result.loginUrl) {
          // Redirect to Keycloak OAuth login
          window.location.href = result.loginUrl;
        } else {
          this.showErrors(result.errors || ['OAuth-Anmeldung fehlgeschlagen']);
        }
      },
      error: (error) => {
        console.error('OAuth initiation error:', error);
        this.snackBar.open('OAuth-Anmeldung konnte nicht gestartet werden.', 'Schließen', {
          duration: 5000
        });
      }
    });
  }

  private showErrors(errors: string[]): void {
    const message = errors.join(', ');
    this.snackBar.open(message, 'Schließen', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}