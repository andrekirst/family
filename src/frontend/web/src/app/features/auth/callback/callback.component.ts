import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatCardModule
  ],
  template: `
    <div class="callback-container">
      <mat-card class="callback-card">
        <mat-card-content>
          <div class="loading-content">
            <mat-spinner diameter="50"></mat-spinner>
            <h2>Anmeldung wird verarbeitet...</h2>
            <p>Bitte warten Sie, während wir Ihre Anmeldung abschließen.</p>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }
    
    .callback-card {
      width: 100%;
      max-width: 400px;
      padding: 40px 20px;
      text-align: center;
    }
    
    .loading-content h2 {
      margin: 20px 0 10px 0;
      color: rgba(0, 0, 0, 0.87);
    }
    
    .loading-content p {
      margin: 0;
      color: rgba(0, 0, 0, 0.6);
    }
  `]
})
export class CallbackComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);


  ngOnInit(): void {
    // Get authorization code and state from URL parameters
    this.route.queryParams.subscribe(params => {
      const authorizationCode = params['code'];
      const state = params['state'];
      const error = params['error'];

      if (error) {
        this.handleError(params['error_description'] || error);
        return;
      }

      if (authorizationCode && state) {
        this.completeLogin(authorizationCode, state);
      } else {
        this.handleError('Fehlende Anmeldeparameter');
      }
    });
  }

  private completeLogin(authorizationCode: string, state: string): void {
    this.authService.completeOAuthLogin(authorizationCode, state).subscribe({
      next: (result) => {
        if (result.success) {
          this.router.navigate(['/dashboard']);
          this.snackBar.open('Erfolgreich angemeldet!', 'Schließen', {
            duration: 3000
          });
        } else {
          this.handleError(result.errors?.join(', ') || 'Anmeldung fehlgeschlagen');
        }
      },
      error: (error) => {
        console.error('OAuth callback error:', error);
        this.handleError('Ein Fehler ist beim Abschluss der Anmeldung aufgetreten');
      }
    });
  }

  private handleError(message: string): void {
    this.snackBar.open(message, 'Schließen', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
    
    // Redirect to login page after showing error
    setTimeout(() => {
      this.router.navigate(['/auth/login']);
    }, 2000);
  }
}