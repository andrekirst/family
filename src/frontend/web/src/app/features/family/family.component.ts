import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-family',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="page-container">
      <mat-card>
        <mat-card-header>
          <mat-icon mat-card-avatar>people</mat-icon>
          <mat-card-title>Familienverwaltung</mat-card-title>
          <mat-card-subtitle>Familienmitglieder und Beziehungen</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>Verwalten Sie hier alle Familienmitglieder, Rollen und Beziehungen.</p>
          <p><em>Diese Funktion wird in einer zukünftigen Version implementiert.</em></p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
  `]
})
export class FamilyComponent {}