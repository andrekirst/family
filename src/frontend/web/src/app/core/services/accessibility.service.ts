import { Injectable, Renderer2, RendererFactory2, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AccessibilitySettings {
  highContrast: boolean;
  reducedMotion: boolean;
  fontSize: 'normal' | 'large' | 'larger';
  screenReaderOptimized: boolean;
}

export interface LiveRegionOptions {
  politeness: 'off' | 'polite' | 'assertive';
  atomic?: boolean;
  relevant?: 'additions' | 'removals' | 'text' | 'all';
  busy?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {
  private renderer: Renderer2;
  private liveRegionContainer: HTMLElement | null = null;
  
  private settingsSubject = new BehaviorSubject<AccessibilitySettings>({
    highContrast: false,
    reducedMotion: this.prefersReducedMotion(),
    fontSize: 'normal',
    screenReaderOptimized: false
  });
  
  public settings$ = this.settingsSubject.asObservable();

  constructor() {
    const rendererFactory = inject(RendererFactory2);
    this.renderer = rendererFactory.createRenderer(null, null);
    this.initializeLiveRegions();
    this.loadUserPreferences();
    this.setupMediaQueryListeners();
  }

  // ARIA Live Regions
  announceToScreenReader(message: string, options: LiveRegionOptions = { politeness: 'polite' }): void {
    if (!this.liveRegionContainer) {
      this.initializeLiveRegions();
    }

    const announcement = this.renderer.createElement('div');
    this.renderer.setAttribute(announcement, 'aria-live', options.politeness);
    this.renderer.setAttribute(announcement, 'aria-atomic', options.atomic ? 'true' : 'false');
    
    if (options.relevant) {
      this.renderer.setAttribute(announcement, 'aria-relevant', options.relevant);
    }
    
    if (options.busy !== undefined) {
      this.renderer.setAttribute(announcement, 'aria-busy', options.busy.toString());
    }

    this.renderer.appendChild(announcement, this.renderer.createText(message));
    this.renderer.appendChild(this.liveRegionContainer, announcement);

    // Remove the announcement after a delay to keep the live region clean
    setTimeout(() => {
      if (this.liveRegionContainer && this.liveRegionContainer.contains(announcement)) {
        this.renderer.removeChild(this.liveRegionContainer, announcement);
      }
    }, 1000);
  }

  // Focus Management
  trapFocus(container: HTMLElement): () => void {
    const focusableElements = this.getFocusableElements(container);
    const firstFocusable = focusableElements[0];
    const lastFocusable = focusableElements[focusableElements.length - 1];

    const handleTabKey = (event: KeyboardEvent) => {
      if (event.key !== 'Tab') return;

      if (event.shiftKey) {
        if (document.activeElement === firstFocusable) {
          event.preventDefault();
          lastFocusable?.focus();
        }
      } else {
        if (document.activeElement === lastFocusable) {
          event.preventDefault();
          firstFocusable?.focus();
        }
      }
    };

    container.addEventListener('keydown', handleTabKey);
    firstFocusable?.focus();

    // Return cleanup function
    return () => {
      container.removeEventListener('keydown', handleTabKey);
    };
  }

  restoreFocus(element: HTMLElement): void {
    // Ensure element is focusable
    if (element && typeof element.focus === 'function') {
      element.focus();
    }
  }

  getFocusableElements(container: HTMLElement): HTMLElement[] {
    const focusableSelectors = [
      'button:not([disabled])',
      'input:not([disabled])',
      'select:not([disabled])',
      'textarea:not([disabled])',
      'a[href]',
      '[tabindex]:not([tabindex="-1"])',
      '[contenteditable="true"]'
    ].join(', ');

    return Array.from(container.querySelectorAll(focusableSelectors)) as HTMLElement[];
  }

  // Keyboard Navigation
  handleArrowNavigation(elements: HTMLElement[], currentIndex: number, direction: 'up' | 'down' | 'left' | 'right'): number {
    let newIndex = currentIndex;
    
    switch (direction) {
      case 'up':
      case 'left':
        newIndex = currentIndex > 0 ? currentIndex - 1 : elements.length - 1;
        break;
      case 'down':
      case 'right':
        newIndex = currentIndex < elements.length - 1 ? currentIndex + 1 : 0;
        break;
    }
    
    elements[newIndex]?.focus();
    return newIndex;
  }

  // ARIA Utilities
  generateId(prefix: string = 'aria'): string {
    return `${prefix}-${Math.random().toString(36).substr(2, 9)}`;
  }

  setAriaLabel(element: HTMLElement, label: string): void {
    this.renderer.setAttribute(element, 'aria-label', label);
  }

  setAriaDescribedBy(element: HTMLElement, describedById: string): void {
    this.renderer.setAttribute(element, 'aria-describedby', describedById);
  }

  setAriaLabelledBy(element: HTMLElement, labelledById: string): void {
    this.renderer.setAttribute(element, 'aria-labelledby', labelledById);
  }

  setAriaExpanded(element: HTMLElement, expanded: boolean): void {
    this.renderer.setAttribute(element, 'aria-expanded', expanded.toString());
  }

  setAriaHidden(element: HTMLElement, hidden: boolean): void {
    if (hidden) {
      this.renderer.setAttribute(element, 'aria-hidden', 'true');
    } else {
      this.renderer.removeAttribute(element, 'aria-hidden');
    }
  }

  // Settings Management
  updateSettings(settings: Partial<AccessibilitySettings>): void {
    const currentSettings = this.settingsSubject.value;
    const newSettings = { ...currentSettings, ...settings };
    this.settingsSubject.next(newSettings);
    this.applySettings(newSettings);
    this.saveUserPreferences(newSettings);
  }

  getSettings(): AccessibilitySettings {
    return this.settingsSubject.value;
  }

  // High Contrast Mode
  toggleHighContrast(): void {
    const current = this.settingsSubject.value.highContrast;
    this.updateSettings({ highContrast: !current });
  }

  // Font Size
  increaseFontSize(): void {
    const current = this.settingsSubject.value.fontSize;
    const sizes: Array<'normal' | 'large' | 'larger'> = ['normal', 'large', 'larger'];
    const currentIndex = sizes.indexOf(current);
    const newSize = sizes[Math.min(currentIndex + 1, sizes.length - 1)];
    this.updateSettings({ fontSize: newSize });
  }

  decreaseFontSize(): void {
    const current = this.settingsSubject.value.fontSize;
    const sizes: Array<'normal' | 'large' | 'larger'> = ['normal', 'large', 'larger'];
    const currentIndex = sizes.indexOf(current);
    const newSize = sizes[Math.max(currentIndex - 1, 0)];
    this.updateSettings({ fontSize: newSize });
  }

  // Private Methods
  private initializeLiveRegions(): void {
    this.liveRegionContainer = this.renderer.createElement('div');
    this.renderer.addClass(this.liveRegionContainer, 'sr-only');
    this.renderer.setAttribute(this.liveRegionContainer, 'aria-live', 'polite');
    this.renderer.setAttribute(this.liveRegionContainer, 'aria-atomic', 'false');
    this.renderer.appendChild(document.body, this.liveRegionContainer);
  }

  private prefersReducedMotion(): boolean {
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }

  private setupMediaQueryListeners(): void {
    const reducedMotionQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    const highContrastQuery = window.matchMedia('(prefers-contrast: high)');

    reducedMotionQuery.addEventListener('change', (e) => {
      this.updateSettings({ reducedMotion: e.matches });
    });

    highContrastQuery.addEventListener('change', (e) => {
      this.updateSettings({ highContrast: e.matches });
    });
  }

  private applySettings(settings: AccessibilitySettings): void {
    const html = document.documentElement;
    
    // High Contrast
    if (settings.highContrast) {
      this.renderer.addClass(html, 'high-contrast');
    } else {
      this.renderer.removeClass(html, 'high-contrast');
    }

    // Reduced Motion
    if (settings.reducedMotion) {
      this.renderer.addClass(html, 'reduce-motion');
    } else {
      this.renderer.removeClass(html, 'reduce-motion');
    }

    // Font Size
    this.renderer.removeClass(html, 'font-normal');
    this.renderer.removeClass(html, 'font-large');
    this.renderer.removeClass(html, 'font-larger');
    this.renderer.addClass(html, `font-${settings.fontSize}`);

    // Screen Reader Optimization
    if (settings.screenReaderOptimized) {
      this.renderer.addClass(html, 'screen-reader-optimized');
    } else {
      this.renderer.removeClass(html, 'screen-reader-optimized');
    }
  }

  private loadUserPreferences(): void {
    try {
      const saved = localStorage.getItem('accessibility-settings');
      if (saved) {
        const settings = JSON.parse(saved);
        this.updateSettings(settings);
      }
    } catch (error) {
      console.warn('Could not load accessibility preferences:', error);
    }
  }

  private saveUserPreferences(settings: AccessibilitySettings): void {
    try {
      localStorage.setItem('accessibility-settings', JSON.stringify(settings));
    } catch (error) {
      console.warn('Could not save accessibility preferences:', error);
    }
  }
}