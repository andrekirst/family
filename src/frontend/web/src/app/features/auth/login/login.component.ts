import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AccessibilityService } from '../../../core/services/accessibility.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading$: Observable<boolean>;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);
  private accessibilityService = inject(AccessibilityService);

  constructor() {
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
      
      // Announce login attempt to screen readers
      this.accessibilityService.announceToScreenReader('Anmeldung wird versucht...', { politeness: 'assertive' });
      
      this.authService.directLogin({ email, password }).subscribe({
        next: (result) => {
          if (result.success) {
            this.accessibilityService.announceToScreenReader('Anmeldung erfolgreich. Sie werden weitergeleitet.', { politeness: 'assertive' });
            this.router.navigate(['/dashboard']);
            this.notificationService.showSuccess('Erfolgreich angemeldet!');
          } else {
            this.accessibilityService.announceToScreenReader('Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Eingaben.', { politeness: 'assertive' });
            this.showErrors(result.errors || ['Anmeldung fehlgeschlagen']);
          }
        },
        error: (error) => {
          console.error('Login error:', error);
          this.accessibilityService.announceToScreenReader('Ein Fehler ist bei der Anmeldung aufgetreten.', { politeness: 'assertive' });
          this.notificationService.showError('Ein Fehler ist aufgetreten. Bitte versuchen Sie es erneut.');
        }
      });
    } else {
      this.accessibilityService.announceToScreenReader('Bitte füllen Sie alle erforderlichen Felder korrekt aus.', { politeness: 'assertive' });
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
        this.notificationService.showError('OAuth-Anmeldung konnte nicht gestartet werden.');
      }
    });
  }

  private showErrors(errors: string[]): void {
    const message = errors.join(', ');
    this.notificationService.showError(message);
  }
}