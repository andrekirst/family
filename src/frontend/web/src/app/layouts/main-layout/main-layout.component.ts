import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { AuthService } from '../../core/auth/auth.service';
import { User } from '../../core/graphql/types';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav 
        #drawer 
        class="sidenav" 
        fixedInViewport
        [attr.role]="(isHandset$ | async) ? 'dialog' : 'navigation'"
        [mode]="(isHandset$ | async) ? 'over' : 'side'"
        [opened]="(isHandset$ | async) === false">
        
        <mat-toolbar class="sidenav-header">
          <span class="app-title">Family</span>
        </mat-toolbar>
        
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>
          
          <a mat-list-item routerLink="/calendar" routerLinkActive="active-link">
            <mat-icon matListItemIcon>event</mat-icon>
            <span matListItemTitle>Kalender</span>
          </a>
          
          <a mat-list-item routerLink="/school" routerLinkActive="active-link">
            <mat-icon matListItemIcon>school</mat-icon>
            <span matListItemTitle>Schule</span>
          </a>
          
          <a mat-list-item routerLink="/kindergarten" routerLinkActive="active-link">
            <mat-icon matListItemIcon>child_care</mat-icon>
            <span matListItemTitle>Kindergarten</span>
          </a>
          
          <a mat-list-item routerLink="/health" routerLinkActive="active-link">
            <mat-icon matListItemIcon>local_hospital</mat-icon>
            <span matListItemTitle>Gesundheit</span>
          </a>
          
          <a mat-list-item routerLink="/family" routerLinkActive="active-link">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Familie</span>
          </a>
          
          <mat-divider></mat-divider>
          
          <a mat-list-item routerLink="/profile" routerLinkActive="active-link">
            <mat-icon matListItemIcon>person</mat-icon>
            <span matListItemTitle>Profil</span>
          </a>
          
          <a mat-list-item routerLink="/settings" routerLinkActive="active-link">
            <mat-icon matListItemIcon>settings</mat-icon>
            <span matListItemTitle>Einstellungen</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      
      <mat-sidenav-content>
        <mat-toolbar color="primary" class="main-toolbar">
          <button
            type="button"
            aria-label="Toggle sidenav"
            mat-icon-button
            (click)="drawer.toggle()"
            *ngIf="isHandset$ | async">
            <mat-icon aria-label="Side nav toggle icon">menu</mat-icon>
          </button>
          
          <span class="toolbar-spacer"></span>
          
          <!-- User Menu -->
          <button mat-icon-button [matMenuTriggerFor]="userMenu">
            <mat-icon>account_circle</mat-icon>
          </button>
          
          <mat-menu #userMenu="matMenu">
            <div class="user-info">
              <div class="user-name">{{ (currentUser$ | async)?.fullName || (currentUser$ | async)?.email }}</div>
              <div class="user-email">{{ (currentUser$ | async)?.email }}</div>
            </div>
            <mat-divider></mat-divider>
            <button mat-menu-item routerLink="/profile">
              <mat-icon>person</mat-icon>
              <span>Profil</span>
            </button>
            <button mat-menu-item routerLink="/settings">
              <mat-icon>settings</mat-icon>
              <span>Einstellungen</span>
            </button>
            <mat-divider></mat-divider>
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              <span>Abmelden</span>
            </button>
          </mat-menu>
        </mat-toolbar>
        
        <main class="main-content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100%;
    }
    
    .sidenav {
      width: 250px;
      background: #fafafa;
    }
    
    .sidenav-header {
      background: #1976d2;
      color: white;
      height: 64px;
      display: flex;
      align-items: center;
      padding: 0 16px;
    }
    
    .app-title {
      font-size: 1.5rem;
      font-weight: 500;
    }
    
    .sidenav .mat-toolbar {
      background: inherit;
    }
    
    .main-toolbar {
      position: sticky;
      top: 0;
      z-index: 1000;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }
    
    .toolbar-spacer {
      flex: 1 1 auto;
    }
    
    .main-content {
      min-height: calc(100vh - 64px);
      background: #f5f5f5;
    }
    
    .active-link {
      background: rgba(25, 118, 210, 0.1) !important;
      color: #1976d2 !important;
    }
    
    .active-link mat-icon {
      color: #1976d2 !important;
    }
    
    .user-info {
      padding: 16px;
      border-bottom: 1px solid rgba(0, 0, 0, 0.12);
    }
    
    .user-name {
      font-weight: 500;
      margin-bottom: 4px;
    }
    
    .user-email {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }
    
    mat-nav-list a {
      transition: background-color 0.2s ease;
    }
    
    mat-nav-list a:hover {
      background: rgba(0, 0, 0, 0.04);
    }
    
    @media (max-width: 768px) {
      .sidenav {
        width: 280px;
      }
    }
  `]
})
export class MainLayoutComponent implements OnInit {
  currentUser$: Observable<User | null>;
  isHandset$: Observable<boolean>;

  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  constructor() {
    this.currentUser$ = this.authService.currentUser$;
    this.isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
      .pipe(
        map(result => result.matches),
        shareReplay()
      );
  }

  ngOnInit(): void {
    // Load current user if not already loaded
    if (!this.authService.currentUser) {
      this.authService.loadCurrentUser().subscribe();
    }
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open('Erfolgreich abgemeldet', 'Schließen', {
            duration: 3000
          });
        }
        // Navigation happens automatically in AuthService.handleLogout()
      },
      error: (error) => {
        console.error('Logout error:', error);
        this.snackBar.open('Fehler beim Abmelden', 'Schließen', {
          duration: 3000
        });
      }
    });
  }
}