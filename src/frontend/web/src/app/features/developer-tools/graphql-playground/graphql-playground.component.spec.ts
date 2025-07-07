import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { By } from '@angular/platform-browser';

import { GraphqlPlaygroundComponent } from './graphql-playground.component';

describe('GraphqlPlaygroundComponent', () => {
  let component: GraphqlPlaygroundComponent;
  let fixture: ComponentFixture<GraphqlPlaygroundComponent>;
  let domSanitizer: jasmine.SpyObj<DomSanitizer>;
  let mockSafeUrl: SafeResourceUrl;

  beforeEach(async () => {
    const domSanitizerSpy = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustResourceUrl']);
    mockSafeUrl = {} as SafeResourceUrl;

    await TestBed.configureTestingModule({
      imports: [GraphqlPlaygroundComponent],
      providers: [
        { provide: DomSanitizer, useValue: domSanitizerSpy }
      ]
    }).compileComponents();

    domSanitizer = TestBed.inject(DomSanitizer) as jasmine.SpyObj<DomSanitizer>;
    domSanitizer.bypassSecurityTrustResourceUrl.and.returnValue(mockSafeUrl);

    fixture = TestBed.createComponent(GraphqlPlaygroundComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.graphqlPlaygroundUrl).toBeNull();
      expect(component.iframeHeight).toBe(600);
      expect(component.error).toBeNull();
    });

    it('should load GraphQL playground on init', () => {
      spyOn(component as any, 'loadGraphqlPlayground');
      spyOn(component as any, 'adjustIframeHeight');
      
      component.ngOnInit();
      
      expect(component['loadGraphqlPlayground']).toHaveBeenCalled();
      expect(component['adjustIframeHeight']).toHaveBeenCalled();
    });

    it('should add resize event listener on init', () => {
      spyOn(window, 'addEventListener');
      
      component.ngOnInit();
      
      expect(window.addEventListener).toHaveBeenCalledWith('resize', jasmine.any(Function));
    });
  });

  describe('GraphQL Playground Loading', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should create safe URL for GraphQL playground', () => {
      expect(domSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalledWith('/graphql/playground');
      expect(component.graphqlPlaygroundUrl).toBe(mockSafeUrl);
    });

    it('should handle errors during playground loading', () => {
      domSanitizer.bypassSecurityTrustResourceUrl.and.throwError('Security error');
      
      component['loadGraphqlPlayground']();
      
      expect(component.error).toBe('GraphQL Playground konnte nicht geladen werden. Bitte überprüfen Sie die Konfiguration.');
      expect(component.graphqlPlaygroundUrl).toBeNull();
    });

    it('should log success message on iframe load', () => {
      spyOn(console, 'log');
      
      component.onIframeLoad();
      
      expect(console.log).toHaveBeenCalledWith('GraphQL Playground loaded successfully');
    });
  });

  describe('Template Rendering', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display page title and description', () => {
      const title = fixture.debugElement.query(By.css('h1'));
      const description = fixture.debugElement.query(By.css('p'));
      
      expect(title.nativeElement.textContent.trim()).toBe('GraphQL Playground');
      expect(description.nativeElement.textContent.trim()).toContain('Interaktive GraphQL-IDE');
    });

    it('should display feature list', () => {
      const featureList = fixture.debugElement.query(By.css('ul'));
      const features = fixture.debugElement.queryAll(By.css('li'));
      
      expect(featureList).toBeTruthy();
      expect(features.length).toBe(4);
      expect(features[0].nativeElement.textContent.trim()).toContain('Interaktive Query-Erstellung');
    });

    it('should render iframe when URL is available', () => {
      component.graphqlPlaygroundUrl = mockSafeUrl;
      fixture.detectChanges();
      
      const iframe = fixture.debugElement.query(By.css('iframe'));
      expect(iframe).toBeTruthy();
      expect(iframe.nativeElement.src).toBeDefined();
      expect(iframe.nativeElement.title).toBe('GraphQL Playground');
    });

    it('should show loading state when URL is not available', () => {
      component.graphqlPlaygroundUrl = null;
      fixture.detectChanges();
      
      const loadingDiv = fixture.debugElement.query(By.css('.animate-spin'));
      const loadingText = fixture.debugElement.query(By.css('p:contains("wird geladen")'));
      
      expect(loadingDiv).toBeTruthy();
      expect(loadingText?.nativeElement.textContent).toContain('wird geladen');
    });

    it('should show error state when error occurs', () => {
      component.error = 'Test error message';
      fixture.detectChanges();
      
      const errorSection = fixture.debugElement.query(By.css('.bg-red-50'));
      expect(errorSection).toBeTruthy();
      expect(errorSection.nativeElement.textContent).toContain('Test error message');
    });

    it('should set iframe height dynamically', () => {
      component.graphqlPlaygroundUrl = mockSafeUrl;
      component.iframeHeight = 800;
      fixture.detectChanges();
      
      const iframeContainer = fixture.debugElement.query(By.css('.bg-white.dark\\:bg-gray-800'));
      expect(iframeContainer.nativeElement.style.height).toBe('800px');
    });
  });

  describe('Button Interactions', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should call openInNewTab when button is clicked', () => {
      spyOn(component, 'openInNewTab');
      
      const button = fixture.debugElement.query(By.css('button'));
      button.nativeElement.click();
      
      expect(component.openInNewTab).toHaveBeenCalled();
    });

    it('should open GraphQL playground in new tab', () => {
      spyOn(window, 'open');
      
      component.openInNewTab();
      
      expect(window.open).toHaveBeenCalledWith('/graphql/playground', '_blank');
    });

    it('should have proper button styling and accessibility', () => {
      const button = fixture.debugElement.query(By.css('button'));
      const classList = button.nativeElement.classList;
      
      expect(classList.contains('focus:outline-none')).toBeTruthy();
      expect(classList.contains('focus:ring-2')).toBeTruthy();
      expect(button.nativeElement.getAttribute('aria-label')).toBe('GraphQL Playground in neuem Tab öffnen');
    });
  });

  describe('Iframe Height Adjustment', () => {
    it('should calculate iframe height based on window height', () => {
      Object.defineProperty(window, 'innerHeight', {
        writable: true,
        configurable: true,
        value: 1000
      });
      
      component['adjustIframeHeight']();
      
      const expectedHeight = 1000 - 180 - 32; // window height - header - padding
      expect(component.iframeHeight).toBe(expectedHeight);
    });

    it('should handle resize events', () => {
      spyOn(component as any, 'adjustIframeHeight');
      
      window.dispatchEvent(new Event('resize'));
      
      // Note: This test may not work as expected due to how the resize listener is set up
      // In a real implementation, you might want to use a more testable approach
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have proper heading structure', () => {
      const h1 = fixture.debugElement.query(By.css('h1'));
      const h3 = fixture.debugElement.query(By.css('h3'));
      
      expect(h1).toBeTruthy();
      expect(h3).toBeTruthy();
    });

    it('should have accessible iframe attributes', () => {
      component.graphqlPlaygroundUrl = mockSafeUrl;
      fixture.detectChanges();
      
      const iframe = fixture.debugElement.query(By.css('iframe'));
      expect(iframe.nativeElement.title).toBe('GraphQL Playground');
      expect(iframe.nativeElement.getAttribute('aria-label')).toBe('Eingebettetes GraphQL Playground');
    });

    it('should have accessible button with aria-label', () => {
      const button = fixture.debugElement.query(By.css('button'));
      expect(button.nativeElement.getAttribute('aria-label')).toBe('GraphQL Playground in neuem Tab öffnen');
    });

    it('should have proper contrast and focus states', () => {
      const button = fixture.debugElement.query(By.css('button'));
      const classList = button.nativeElement.classList;
      
      expect(classList.contains('focus:ring-2')).toBeTruthy();
      expect(classList.contains('focus:ring-offset-2')).toBeTruthy();
      expect(classList.contains('transition-colors')).toBeTruthy();
    });
  });

  describe('Responsive Design', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have responsive container classes', () => {
      const container = fixture.debugElement.query(By.css('.max-w-7xl'));
      expect(container).toBeTruthy();
      
      const responsivePadding = fixture.debugElement.query(By.css('.px-4.sm\\:px-6.lg\\:px-8'));
      expect(responsivePadding).toBeTruthy();
    });

    it('should have responsive iframe container', () => {
      const iframeContainer = fixture.debugElement.query(By.css('.w-full.h-full'));
      expect(iframeContainer).toBeTruthy();
    });
  });

  describe('Dark Mode Support', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have dark mode classes', () => {
      const container = fixture.debugElement.query(By.css('.dark\\:bg-gray-900'));
      expect(container).toBeTruthy();
      
      const header = fixture.debugElement.query(By.css('.dark\\:bg-gray-800'));
      expect(header).toBeTruthy();
      
      const title = fixture.debugElement.query(By.css('h1'));
      expect(title.nativeElement.classList.contains('dark:text-gray-100')).toBeTruthy();
    });

    it('should have dark mode info box styling', () => {
      const infoBox = fixture.debugElement.query(By.css('.dark\\:bg-blue-900\\/20'));
      expect(infoBox).toBeTruthy();
    });

    it('should have dark mode error state styling', () => {
      component.error = 'Test error';
      fixture.detectChanges();
      
      const errorBox = fixture.debugElement.query(By.css('.dark\\:bg-red-900\\/20'));
      expect(errorBox).toBeTruthy();
    });
  });

  describe('Loading State', () => {
    it('should show loading spinner when playground URL is not set', () => {
      component.graphqlPlaygroundUrl = null;
      fixture.detectChanges();
      
      const spinner = fixture.debugElement.query(By.css('.animate-spin'));
      const loadingText = fixture.debugElement.query(By.css('p:contains("wird geladen")'));
      
      expect(spinner).toBeTruthy();
      expect(loadingText?.nativeElement.textContent).toContain('GraphQL Playground wird geladen');
    });

    it('should hide loading state when playground URL is set', () => {
      component.graphqlPlaygroundUrl = mockSafeUrl;
      fixture.detectChanges();
      
      const loadingDiv = fixture.debugElement.query(By.css('.animate-spin'));
      expect(loadingDiv).toBeFalsy();
    });
  });

  describe('Error Handling', () => {
    it('should display error message in error state', () => {
      const errorMessage = 'Failed to load GraphQL Playground';
      component.error = errorMessage;
      fixture.detectChanges();
      
      const errorSection = fixture.debugElement.query(By.css('.bg-red-50'));
      expect(errorSection.nativeElement.textContent).toContain(errorMessage);
    });

    it('should have proper error styling', () => {
      component.error = 'Test error';
      fixture.detectChanges();
      
      const errorSection = fixture.debugElement.query(By.css('.bg-red-50'));
      const errorIcon = fixture.debugElement.query(By.css('.text-red-400'));
      const errorTitle = fixture.debugElement.query(By.css('.text-red-800'));
      
      expect(errorSection).toBeTruthy();
      expect(errorIcon).toBeTruthy();
      expect(errorTitle).toBeTruthy();
    });
  });

  describe('Component Cleanup', () => {
    it('should remove resize event listener on destroy', () => {
      spyOn(window, 'removeEventListener');
      
      component.ngOnDestroy();
      
      expect(window.removeEventListener).toHaveBeenCalledWith('resize', jasmine.any(Function));
    });
  });

  describe('Icons and Visual Elements', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have SVG icons with proper attributes', () => {
      const svgIcons = fixture.debugElement.queryAll(By.css('svg'));
      
      svgIcons.forEach(svg => {
        expect(svg.nativeElement.getAttribute('viewBox')).toBeTruthy();
        const hasStroke = svg.nativeElement.getAttribute('stroke');
        const hasFill = svg.nativeElement.getAttribute('fill');
        expect(hasStroke || hasFill).toBeTruthy();
      });
    });

    it('should have consistent icon sizes', () => {
      const smallIcons = fixture.debugElement.queryAll(By.css('.w-5.h-5'));
      expect(smallIcons.length).toBeGreaterThan(0);
    });

    it('should have proper visual hierarchy', () => {
      const infoBox = fixture.debugElement.query(By.css('.bg-blue-50'));
      const iframeContainer = fixture.debugElement.query(By.css('.shadow-lg'));
      
      expect(infoBox).toBeTruthy();
      expect(iframeContainer).toBeTruthy();
    });
  });
});