import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';

import { ServerErrorComponent } from './server-error.component';

describe('ServerErrorComponent', () => {
  let component: ServerErrorComponent;
  let fixture: ComponentFixture<ServerErrorComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ServerErrorComponent,
        RouterTestingModule.withRoutes([
          { path: 'dashboard', component: ServerErrorComponent }
        ])
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(ServerErrorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should generate timestamp on initialization', () => {
      expect(component.timestamp).toBeDefined();
      expect(typeof component.timestamp).toBe('string');
      expect(() => new Date(component.timestamp)).not.toThrow();
    });

    it('should generate error ID on initialization', () => {
      expect(component.errorId).toBeDefined();
      expect(typeof component.errorId).toBe('string');
      expect(component.errorId.length).toBeGreaterThan(0);
    });

    it('should generate unique error IDs', () => {
      const component2 = new ServerErrorComponent();
      expect(component.errorId).not.toBe(component2.errorId);
    });
  });

  describe('Error ID Generation', () => {
    it('should generate error ID with correct format', () => {
      const errorId = component.errorId;
      expect(errorId).toMatch(/^[A-Z0-9]+$/);
      expect(errorId.length).toBe(7);
    });

    it('should generate different error IDs on multiple calls', () => {
      const errorIds = Array.from({ length: 10 }, () => 
        component['generateErrorId']()
      );
      
      const uniqueIds = new Set(errorIds);
      expect(uniqueIds.size).toBe(errorIds.length);
    });
  });

  describe('Template Rendering', () => {
    it('should display 500 heading', () => {
      const heading = fixture.debugElement.query(By.css('h1'));
      expect(heading.nativeElement.textContent.trim()).toBe('500');
    });

    it('should display error title', () => {
      const title = fixture.debugElement.query(By.css('h2'));
      expect(title.nativeElement.textContent.trim()).toBe('Serverfehler');
    });

    it('should display error description', () => {
      const description = fixture.debugElement.query(By.css('p'));
      expect(description.nativeElement.textContent.trim())
        .toContain('Es ist ein unerwarteter Fehler aufgetreten');
    });

    it('should display 500 illustration', () => {
      const illustration = fixture.debugElement.query(By.css('svg.h-48'));
      expect(illustration).toBeTruthy();
    });

    it('should display system status', () => {
      const statusSection = fixture.debugElement.query(By.css('.bg-white.dark\\:bg-gray-800'));
      expect(statusSection).toBeTruthy();
      expect(statusSection.nativeElement.textContent).toContain('System-Status: Fehler');
    });

    it('should have proper styling classes', () => {
      const container = fixture.debugElement.query(By.css('.min-h-screen'));
      expect(container).toBeTruthy();
      
      const redGradientBg = fixture.debugElement.query(By.css('.from-red-50'));
      expect(redGradientBg).toBeTruthy();
    });
  });

  describe('Technical Details Section', () => {
    it('should have collapsible technical details', () => {
      const details = fixture.debugElement.query(By.css('details'));
      const summary = fixture.debugElement.query(By.css('summary'));
      
      expect(details).toBeTruthy();
      expect(summary).toBeTruthy();
      expect(summary.nativeElement.textContent.trim()).toBe('Technische Details anzeigen');
    });

    it('should display timestamp in technical details', () => {
      const detailsContent = fixture.debugElement.query(By.css('details div'));
      expect(detailsContent.nativeElement.textContent).toContain(`Zeitstempel: ${component.timestamp}`);
    });

    it('should display error ID in technical details', () => {
      const detailsContent = fixture.debugElement.query(By.css('details div'));
      expect(detailsContent.nativeElement.textContent).toContain(`Fehler-ID: ${component.errorId}`);
    });

    it('should display status in technical details', () => {
      const detailsContent = fixture.debugElement.query(By.css('details div'));
      expect(detailsContent.nativeElement.textContent).toContain('Status: Internal Server Error (500)');
    });

    it('should have proper styling for technical details', () => {
      const detailsContent = fixture.debugElement.query(By.css('details div'));
      const classList = detailsContent.nativeElement.classList;
      
      expect(classList.contains('bg-gray-100')).toBeTruthy();
      expect(classList.contains('font-mono')).toBeTruthy();
      expect(classList.contains('text-xs')).toBeTruthy();
    });
  });

  describe('Navigation Elements', () => {
    it('should have reload button with correct onclick', () => {
      const reloadButton = fixture.debugElement.query(By.css('button[onclick="window.location.reload()"]'));
      expect(reloadButton).toBeTruthy();
      expect(reloadButton.nativeElement.textContent.trim()).toContain('Seite neu laden');
    });

    it('should have dashboard link with correct routerLink', () => {
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      expect(dashboardLink).toBeTruthy();
      expect(dashboardLink.nativeElement.textContent.trim()).toContain('Zur Startseite');
    });

    it('should have support email link', () => {
      const supportLink = fixture.debugElement.query(By.css('a[href="mailto:support@family.app"]'));
      expect(supportLink).toBeTruthy();
      expect(supportLink.nativeElement.textContent.trim()).toBe('Kontaktieren Sie den Support');
    });
  });

  describe('Status Indicator', () => {
    it('should have animated status indicator', () => {
      const statusIndicator = fixture.debugElement.query(By.css('.w-3.h-3.bg-red-500'));
      expect(statusIndicator).toBeTruthy();
      expect(statusIndicator.nativeElement.classList.contains('animate-pulse')).toBeTruthy();
    });

    it('should display system status text', () => {
      const statusText = fixture.debugElement.query(By.css('.text-gray-600.dark\\:text-gray-400'));
      expect(statusText.nativeElement.textContent.trim()).toContain('System-Status: Fehler');
    });
  });

  describe('Button Functionality', () => {
    it('should have reload button with window.location.reload', () => {
      const reloadButton = fixture.debugElement.query(By.css('button'));
      expect(reloadButton.nativeElement.getAttribute('onclick')).toBe('window.location.reload()');
    });

    it('should navigate to dashboard when dashboard link is clicked', () => {
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      expect(dashboardLink.nativeElement.getAttribute('routerLink')).toBe('/dashboard');
    });
  });

  describe('Accessibility', () => {
    it('should have proper heading hierarchy', () => {
      const h1 = fixture.debugElement.query(By.css('h1'));
      const h2 = fixture.debugElement.query(By.css('h2'));
      
      expect(h1).toBeTruthy();
      expect(h2).toBeTruthy();
    });

    it('should have accessible button and link text', () => {
      const reloadButton = fixture.debugElement.query(By.css('button'));
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      const supportLink = fixture.debugElement.query(By.css('a[href^="mailto:"]'));
      
      expect(reloadButton.nativeElement.textContent.trim()).toContain('Seite neu laden');
      expect(dashboardLink.nativeElement.textContent.trim()).toContain('Zur Startseite');
      expect(supportLink.nativeElement.textContent.trim()).toBe('Kontaktieren Sie den Support');
    });

    it('should have focus styles for interactive elements', () => {
      const interactiveElements = fixture.debugElement.queryAll(By.css('a, button, summary'));
      
      interactiveElements.forEach(element => {
        const classList = element.nativeElement.classList;
        if (element.nativeElement.tagName.toLowerCase() !== 'summary') {
          expect(classList.contains('focus:outline-none')).toBeTruthy();
          expect(classList.contains('focus:ring-2')).toBeTruthy();
        }
      });
    });

    it('should have proper semantic structure', () => {
      const details = fixture.debugElement.query(By.css('details'));
      const summary = fixture.debugElement.query(By.css('summary'));
      
      expect(details).toBeTruthy();
      expect(summary).toBeTruthy();
      expect(summary.nativeElement.classList.contains('cursor-pointer')).toBeTruthy();
    });
  });

  describe('Responsive Design', () => {
    it('should have responsive classes', () => {
      const container = fixture.debugElement.query(By.css('.max-w-md'));
      expect(container).toBeTruthy();
      
      const flexContainer = fixture.debugElement.query(By.css('.flex.flex-col.sm\\:flex-row'));
      expect(flexContainer).toBeTruthy();
      
      const responsivePadding = fixture.debugElement.query(By.css('.px-4.sm\\:px-6.lg\\:px-8'));
      expect(responsivePadding).toBeTruthy();
    });

    it('should have responsive button layout', () => {
      const buttonContainer = fixture.debugElement.query(By.css('.flex.flex-col.sm\\:flex-row'));
      expect(buttonContainer).toBeTruthy();
      
      const gapClasses = buttonContainer.nativeElement.classList;
      expect(gapClasses.contains('gap-4')).toBeTruthy();
    });
  });

  describe('Dark Mode Support', () => {
    it('should have dark mode classes', () => {
      const container = fixture.debugElement.query(By.css('.dark\\:from-gray-900'));
      expect(container).toBeTruthy();
      
      const heading = fixture.debugElement.query(By.css('h2'));
      expect(heading.nativeElement.classList.contains('dark:text-gray-100')).toBeTruthy();
      
      const description = fixture.debugElement.query(By.css('p'));
      expect(description.nativeElement.classList.contains('dark:text-gray-400')).toBeTruthy();
    });

    it('should have dark mode button styles', () => {
      const reloadButton = fixture.debugElement.query(By.css('button'));
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      
      expect(reloadButton.nativeElement.classList.contains('dark:bg-red-500')).toBeTruthy();
      expect(dashboardLink.nativeElement.classList.contains('dark:bg-gray-700')).toBeTruthy();
    });

    it('should have dark mode technical details styling', () => {
      const detailsContent = fixture.debugElement.query(By.css('details div'));
      expect(detailsContent.nativeElement.classList.contains('dark:bg-gray-800')).toBeTruthy();
      expect(detailsContent.nativeElement.classList.contains('dark:text-gray-300')).toBeTruthy();
    });
  });

  describe('Animation Classes', () => {
    it('should have animation classes for 500 heading', () => {
      const heading = fixture.debugElement.query(By.css('h1'));
      expect(heading.nativeElement.classList.contains('animate-pulse')).toBeTruthy();
    });

    it('should have animation classes for status indicator', () => {
      const statusDot = fixture.debugElement.query(By.css('.w-3.h-3.bg-red-500'));
      expect(statusDot.nativeElement.classList.contains('animate-pulse')).toBeTruthy();
    });

    it('should have transition classes for interactive elements', () => {
      const interactiveElements = fixture.debugElement.queryAll(By.css('a, button'));
      
      interactiveElements.forEach(element => {
        expect(element.nativeElement.classList.contains('transition-colors')).toBeTruthy();
      });
    });
  });

  describe('Error Styling', () => {
    it('should use red color scheme for error state', () => {
      const heading = fixture.debugElement.query(By.css('h1'));
      expect(heading.nativeElement.classList.contains('from-red-600')).toBeTruthy();
      
      const reloadButton = fixture.debugElement.query(By.css('button'));
      expect(reloadButton.nativeElement.classList.contains('bg-red-600')).toBeTruthy();
      
      const supportLink = fixture.debugElement.query(By.css('a[href^="mailto:"]'));
      expect(supportLink.nativeElement.classList.contains('text-red-600')).toBeTruthy();
    });

    it('should have proper contrast for error colors', () => {
      const reloadButton = fixture.debugElement.query(By.css('button'));
      expect(reloadButton.nativeElement.classList.contains('text-white')).toBeTruthy();
      
      const container = fixture.debugElement.query(By.css('.from-red-50'));
      expect(container).toBeTruthy();
    });
  });

  describe('Icon Accessibility', () => {
    it('should have SVG icons with proper attributes', () => {
      const svgIcons = fixture.debugElement.queryAll(By.css('svg'));
      
      svgIcons.forEach(svg => {
        expect(svg.nativeElement.getAttribute('viewBox')).toBeTruthy();
        const hasStroke = svg.nativeElement.getAttribute('stroke');
        expect(hasStroke).toBeTruthy();
      });
    });

    it('should have consistent icon sizes', () => {
      const smallIcons = fixture.debugElement.queryAll(By.css('.w-5.h-5'));
      const largeIcon = fixture.debugElement.query(By.css('.h-48.w-48'));
      
      expect(smallIcons.length).toBeGreaterThan(0);
      expect(largeIcon).toBeTruthy();
    });
  });

  describe('Content Structure', () => {
    it('should have proper content sections', () => {
      const sections = [
        '.mb-8', // 500 section, Error message section, Illustration section
        '.flex', // Action buttons section
        'details', // Technical details section
        '.mt-8'  // Help text section
      ];
      
      sections.forEach(selector => {
        const section = fixture.debugElement.query(By.css(selector));
        expect(section).toBeTruthy();
      });
    });

    it('should have proper text hierarchy', () => {
      const largeText = fixture.debugElement.query(By.css('.text-9xl'));
      const titleText = fixture.debugElement.query(By.css('.text-3xl'));
      const bodyText = fixture.debugElement.query(By.css('.text-lg'));
      const smallText = fixture.debugElement.query(By.css('.text-sm'));
      const extraSmallText = fixture.debugElement.query(By.css('.text-xs'));
      
      expect(largeText).toBeTruthy(); // 500
      expect(titleText).toBeTruthy(); // Title
      expect(bodyText).toBeTruthy(); // Description
      expect(smallText).toBeTruthy(); // Help text
      expect(extraSmallText).toBeTruthy(); // Technical details
    });
  });

  describe('Email Link', () => {
    it('should have correctly formatted mailto link', () => {
      const emailLink = fixture.debugElement.query(By.css('a[href="mailto:support@family.app"]'));
      expect(emailLink).toBeTruthy();
      expect(emailLink.nativeElement.href).toBe('mailto:support@family.app');
    });

    it('should have proper styling for email link', () => {
      const emailLink = fixture.debugElement.query(By.css('a[href="mailto:support@family.app"]'));
      const classList = emailLink.nativeElement.classList;
      
      expect(classList.contains('text-red-600')).toBeTruthy();
      expect(classList.contains('hover:text-red-700')).toBeTruthy();
      expect(classList.contains('font-medium')).toBeTruthy();
    });
  });
});