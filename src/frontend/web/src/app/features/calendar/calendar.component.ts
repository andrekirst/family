import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="page-container">
      <mat-card>
        <mat-card-header>
          <mat-icon mat-card-avatar>event</mat-icon>
          <mat-card-title>Familienkalender</mat-card-title>
          <mat-card-subtitle>Termine und Ereignisse verwalten</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>Hier werden alle Familientermine angezeigt und verwaltet.</p>
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
export class CalendarComponent {}