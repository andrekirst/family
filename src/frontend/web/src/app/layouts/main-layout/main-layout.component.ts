import { Component, OnInit, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { AuthService } from '../../core/auth/auth.service';
import { User } from '../../core/graphql/types';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher/language-switcher.component';
import { I18nPipe } from '../../shared/pipes/i18n.pipe';
import { I18nService } from '../../core/services/i18n.service';
import { NotificationService } from '../../core/services/notification.service';
import { AccessibilityService } from '../../core/services/accessibility.service';
import { KeyboardNavigationDirective } from '../../core/directives/keyboard-navigation.directive';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    LanguageSwitcherComponent,
    I18nPipe,
    KeyboardNavigationDirective
  ],
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit {
  currentUser$: Observable<User | null>;
  isHandset$: Observable<boolean>;
  sidebarOpen = false;
  userMenuOpen = false;

  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);
  private i18nService = inject(I18nService);
  private accessibilityService = inject(AccessibilityService);

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

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    // Close user menu if clicking outside
    if (this.userMenuOpen && !this.isClickInsideUserMenu(event)) {
      this.userMenuOpen = false;
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    // Close sidebar on mobile when resizing to larger screen
    this.isHandset$.subscribe(isHandset => {
      if (!isHandset) {
        this.sidebarOpen = false;
      }
    });
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.sidebarOpen = false;
    this.userMenuOpen = false;
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
    
    // Announce sidebar state to screen readers
    const message = this.sidebarOpen 
      ? this.i18nService.translate('nav.sidebarOpened') 
      : this.i18nService.translate('nav.sidebarClosed');
    this.accessibilityService.announceToScreenReader(message);
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  toggleUserMenu(): void {
    this.userMenuOpen = !this.userMenuOpen;
    
    // Announce menu state to screen readers
    const message = this.userMenuOpen
      ? this.i18nService.translate('nav.userMenuOpened')
      : this.i18nService.translate('nav.userMenuClosed');
    this.accessibilityService.announceToScreenReader(message);
  }

  closeUserMenu(): void {
    this.userMenuOpen = false;
  }

  skipToMainContent(): void {
    const mainContent = document.querySelector('main');
    if (mainContent) {
      mainContent.focus();
      this.accessibilityService.announceToScreenReader(
        this.i18nService.translate('nav.skippedToMainContent')
      );
    }
  }

  skipToNavigation(): void {
    const navigation = document.querySelector('nav[role="navigation"]');
    if (navigation) {
      const firstLink = navigation.querySelector('a, button');
      if (firstLink) {
        (firstLink as HTMLElement).focus();
        this.accessibilityService.announceToScreenReader(
          this.i18nService.translate('nav.skippedToNavigation')
        );
      }
    }
  }

  private isClickInsideUserMenu(event: Event): boolean {
    const target = event.target as HTMLElement;
    const userMenu = document.querySelector('.user-menu');
    return userMenu?.contains(target) || false;
  }

  logout(): void {
    this.closeUserMenu();
    this.authService.logout().subscribe({
      next: (result) => {
        if (result.success) {
          this.notificationService.showSuccess(
            this.i18nService.translate('auth.logoutSuccess')
          );
        }
        // Navigation happens automatically in AuthService.handleLogout()
      },
      error: (error) => {
        console.error('Logout error:', error);
        this.notificationService.showError(
          this.i18nService.translate('auth.loginFailed')
        );
      }
    });
  }
}