import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { User } from '../../core/graphql/types';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatGridListModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="welcome-section">
        <h1>Willkommen, {{ (currentUser$ | async)?.firstName || 'Benutzer' }}!</h1>
        <p>Verwalten Sie Ihre Familie effizienter mit der Family-Plattform.</p>
      </div>

      <mat-grid-list cols="4" rowHeight="200px" gutterSize="16px" class="dashboard-grid">
        <!-- Termine -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>event</mat-icon>
              <mat-card-title>Termine</mat-card-title>
              <mat-card-subtitle>Familienkalender verwalten</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Verwalten Sie alle wichtigen Termine für die Familie zentral.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>add</mat-icon>
                Termin hinzufügen
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>

        <!-- Schule -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>school</mat-icon>
              <mat-card-title>Schule</mat-card-title>
              <mat-card-subtitle>Schulorganisation</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Stundenpläne, Noten, Hausaufgaben und Elternabende verwalten.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>assignment</mat-icon>
                Hausaufgaben
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>

        <!-- Kindergarten -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>child_care</mat-icon>
              <mat-card-title>Kindergarten</mat-card-title>
              <mat-card-subtitle>Kindergartenmanagement</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Essensversorgung, Termine und Aktivitäten koordinieren.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>restaurant</mat-icon>
                Essensplan
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>

        <!-- Gesundheit -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>local_hospital</mat-icon>
              <mat-card-title>Gesundheit</mat-card-title>
              <mat-card-subtitle>Medizinische Verwaltung</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Arztbesuche, Medikamente, Impfungen und Erinnerungen.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>medication</mat-icon>
                Medikamente
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>

        <!-- Benutzerverwaltung -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>people</mat-icon>
              <mat-card-title>Familie</mat-card-title>
              <mat-card-subtitle>Familienmitglieder</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Familienmitglieder, Rollen und Beziehungen verwalten.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>person_add</mat-icon>
                Mitglied hinzufügen
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>

        <!-- Einstellungen -->
        <mat-grid-tile colspan="2" rowspan="1">
          <mat-card class="dashboard-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>settings</mat-icon>
              <mat-card-title>Einstellungen</mat-card-title>
              <mat-card-subtitle>Anwendungseinstellungen</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>Sprache, Design und Benachrichtigungen anpassen.</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary">
                <mat-icon>tune</mat-icon>
                Konfigurieren
              </button>
            </mat-card-actions>
          </mat-card>
        </mat-grid-tile>
      </mat-grid-list>

      <div class="stats-section">
        <mat-card class="stats-card">
          <mat-card-header>
            <mat-card-title>Schnellübersicht</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stats-grid">
              <div class="stat-item">
                <mat-icon>event</mat-icon>
                <span class="stat-number">5</span>
                <span class="stat-label">Termine heute</span>
              </div>
              <div class="stat-item">
                <mat-icon>assignment</mat-icon>
                <span class="stat-number">3</span>
                <span class="stat-label">Offene Aufgaben</span>
              </div>
              <div class="stat-item">
                <mat-icon>notification_important</mat-icon>
                <span class="stat-number">2</span>
                <span class="stat-label">Erinnerungen</span>
              </div>
              <div class="stat-item">
                <mat-icon>people</mat-icon>
                <span class="stat-number">4</span>
                <span class="stat-label">Familienmitglieder</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .welcome-section {
      margin-bottom: 32px;
    }
    
    .welcome-section h1 {
      font-size: 2rem;
      font-weight: 500;
      margin-bottom: 8px;
      color: rgba(0, 0, 0, 0.87);
    }
    
    .welcome-section p {
      font-size: 1.1rem;
      color: rgba(0, 0, 0, 0.6);
      margin: 0;
    }
    
    .dashboard-grid {
      margin-bottom: 32px;
    }
    
    .dashboard-card {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      transition: transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out;
    }
    
    .dashboard-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
    }
    
    .dashboard-card mat-card-content {
      flex: 1;
    }
    
    .dashboard-card mat-card-header {
      padding-bottom: 16px;
    }
    
    .dashboard-card mat-card-header mat-icon {
      color: #1976d2;
    }
    
    .stats-section {
      margin-top: 32px;
    }
    
    .stats-card {
      padding: 16px;
    }
    
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 24px;
      margin-top: 16px;
    }
    
    .stat-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 16px;
      border-radius: 8px;
      background: rgba(25, 118, 210, 0.04);
    }
    
    .stat-item mat-icon {
      color: #1976d2;
      margin-bottom: 8px;
    }
    
    .stat-number {
      font-size: 2rem;
      font-weight: 600;
      color: #1976d2;
      margin-bottom: 4px;
    }
    
    .stat-label {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }
    
    @media (max-width: 768px) {
      .dashboard-grid {
        grid-template-columns: 1fr;
      }
      
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }
      
      .dashboard-container {
        padding: 16px;
      }
    }
  `]
})
export class DashboardComponent {
  currentUser$: Observable<User | null>;

  private authService = inject(AuthService);

  constructor() {
    this.currentUser$ = this.authService.currentUser$;
  }
}