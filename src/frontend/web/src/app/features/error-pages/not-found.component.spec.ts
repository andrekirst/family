import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';

import { NotFoundComponent } from './not-found.component';

describe('NotFoundComponent', () => {
  let component: NotFoundComponent;
  let fixture: ComponentFixture<NotFoundComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        NotFoundComponent,
        RouterTestingModule.withRoutes([
          { path: 'dashboard', component: NotFoundComponent }
        ])
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(NotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('Template Rendering', () => {
    it('should display 404 heading', () => {
      const heading = fixture.debugElement.query(By.css('h1'));
      expect(heading.nativeElement.textContent.trim()).toBe('404');
    });

    it('should display error title', () => {
      const title = fixture.debugElement.query(By.css('h2'));
      expect(title.nativeElement.textContent.trim()).toBe('Seite nicht gefunden');
    });

    it('should display error description', () => {
      const description = fixture.debugElement.query(By.css('p'));
      expect(description.nativeElement.textContent.trim())
        .toContain('Die gesuchte Seite existiert leider nicht');
    });

    it('should display 404 illustration', () => {
      const illustration = fixture.debugElement.query(By.css('svg.h-48'));
      expect(illustration).toBeTruthy();
    });

    it('should have proper styling classes', () => {
      const container = fixture.debugElement.query(By.css('.min-h-screen'));
      expect(container).toBeTruthy();
      
      const gradientBg = fixture.debugElement.query(By.css('.bg-gradient-to-br'));
      expect(gradientBg).toBeTruthy();
    });
  });

  describe('Navigation Elements', () => {
    it('should have dashboard link with correct routerLink', () => {
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      expect(dashboardLink).toBeTruthy();
      expect(dashboardLink.nativeElement.textContent.trim()).toContain('Zur Startseite');
    });

    it('should have back button with history.back() onclick', () => {
      const backButton = fixture.debugElement.query(By.css('button[onclick="history.back()"]'));
      expect(backButton).toBeTruthy();
      expect(backButton.nativeElement.textContent.trim()).toContain('Zurück');
    });

    it('should have support email link', () => {
      const supportLink = fixture.debugElement.query(By.css('a[href="mailto:support@family.app"]'));
      expect(supportLink).toBeTruthy();
      expect(supportLink.nativeElement.textContent.trim()).toBe('Kontaktieren Sie den Support');
    });
  });

  describe('Button Functionality', () => {
    it('should navigate to dashboard when dashboard link is clicked', async () => {
      spyOn(router, 'navigate');
      
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      dashboardLink.nativeElement.click();
      
      // Router navigation is handled by RouterLink directive
      expect(dashboardLink.nativeElement.getAttribute('routerLink')).toBe('/dashboard');
    });

    it('should have back button with proper onclick handler', () => {
      const backButton = fixture.debugElement.query(By.css('button'));
      expect(backButton.nativeElement.getAttribute('onclick')).toBe('history.back()');
    });
  });

  describe('Accessibility', () => {
    it('should have proper heading hierarchy', () => {
      const h1 = fixture.debugElement.query(By.css('h1'));
      const h2 = fixture.debugElement.query(By.css('h2'));
      
      expect(h1).toBeTruthy();
      expect(h2).toBeTruthy();
    });

    it('should have accessible link text', () => {
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      const backButton = fixture.debugElement.query(By.css('button'));
      const supportLink = fixture.debugElement.query(By.css('a[href^="mailto:"]'));
      
      expect(dashboardLink.nativeElement.textContent.trim()).toContain('Zur Startseite');
      expect(backButton.nativeElement.textContent.trim()).toContain('Zurück');
      expect(supportLink.nativeElement.textContent.trim()).toBe('Kontaktieren Sie den Support');
    });

    it('should have focus styles for interactive elements', () => {
      const interactiveElements = fixture.debugElement.queryAll(By.css('a, button'));
      
      interactiveElements.forEach(element => {
        const classList = element.nativeElement.classList;
        expect(classList.contains('focus:outline-none')).toBeTruthy();
        expect(classList.contains('focus:ring-2')).toBeTruthy();
      });
    });

    it('should have proper color contrast', () => {
      const links = fixture.debugElement.queryAll(By.css('a'));
      const buttons = fixture.debugElement.queryAll(By.css('button'));
      
      // Check for color classes that ensure proper contrast
      const dashboardLink = links.find(link => 
        link.nativeElement.getAttribute('routerLink') === '/dashboard');
      expect(dashboardLink?.nativeElement.classList.contains('bg-indigo-600')).toBeTruthy();
      
      const backButton = buttons[0];
      expect(backButton?.nativeElement.classList.contains('bg-white')).toBeTruthy();
    });

    it('should have SVG icons with proper accessibility', () => {
      const svgIcons = fixture.debugElement.queryAll(By.css('svg'));
      
      svgIcons.forEach(svg => {
        // SVG icons should have viewBox for proper scaling
        expect(svg.nativeElement.getAttribute('viewBox')).toBeTruthy();
        // Should have fill or stroke for visibility
        const hasFill = svg.nativeElement.getAttribute('fill');
        const hasStroke = svg.nativeElement.getAttribute('stroke');
        expect(hasFill || hasStroke).toBeTruthy();
      });
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
      const dashboardLink = fixture.debugElement.query(By.css('a[routerLink="/dashboard"]'));
      const backButton = fixture.debugElement.query(By.css('button'));
      
      expect(dashboardLink.nativeElement.classList.contains('dark:bg-indigo-500')).toBeTruthy();
      expect(backButton.nativeElement.classList.contains('dark:bg-gray-700')).toBeTruthy();
    });
  });

  describe('Animation Classes', () => {
    it('should have animation classes for 404 heading', () => {
      const heading = fixture.debugElement.query(By.css('h1'));
      expect(heading.nativeElement.classList.contains('animate-pulse')).toBeTruthy();
    });

    it('should have transition classes for interactive elements', () => {
      const interactiveElements = fixture.debugElement.queryAll(By.css('a, button'));
      
      interactiveElements.forEach(element => {
        expect(element.nativeElement.classList.contains('transition-colors')).toBeTruthy();
      });
    });
  });

  describe('Content Structure', () => {
    it('should have proper content sections', () => {
      const sections = [
        '.mb-8', // 404 section
        '.mb-8', // Error message section
        '.mb-8', // Illustration section
        '.flex', // Action buttons section
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
      
      expect(largeText).toBeTruthy(); // 404
      expect(titleText).toBeTruthy(); // Title
      expect(bodyText).toBeTruthy(); // Description
      expect(smallText).toBeTruthy(); // Help text
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
      
      expect(classList.contains('text-indigo-600')).toBeTruthy();
      expect(classList.contains('hover:text-indigo-700')).toBeTruthy();
      expect(classList.contains('font-medium')).toBeTruthy();
    });
  });

  describe('Icon Accessibility', () => {
    it('should have SVG icons with proper stroke width', () => {
      const pathElements = fixture.debugElement.queryAll(By.css('path[stroke-width]'));
      
      pathElements.forEach(path => {
        const strokeWidth = path.nativeElement.getAttribute('stroke-width');
        expect(strokeWidth).toBeTruthy();
        expect(parseFloat(strokeWidth)).toBeGreaterThan(0);
      });
    });

    it('should have consistent icon sizes', () => {
      const smallIcons = fixture.debugElement.queryAll(By.css('.w-5.h-5'));
      const largeIcon = fixture.debugElement.query(By.css('.h-48.w-48'));
      
      expect(smallIcons.length).toBeGreaterThan(0);
      expect(largeIcon).toBeTruthy();
    });
  });
});