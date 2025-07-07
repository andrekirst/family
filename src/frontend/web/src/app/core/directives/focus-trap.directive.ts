import { Directive, ElementRef, Input, OnDestroy, OnInit, inject } from '@angular/core';
import { AccessibilityService } from '../services/accessibility.service';

@Directive({
  selector: '[appFocusTrap]',
  standalone: true
})
export class FocusTrapDirective implements OnInit, OnDestroy {
  @Input() appFocusTrap: boolean = true;
  @Input() focusTrapReturnElement: HTMLElement | null = null;
  @Input() focusTrapInitialFocus: HTMLElement | null = null;
  
  private elementRef = inject(ElementRef);
  private accessibilityService = inject(AccessibilityService);
  private cleanupFocusTrap: (() => void) | null = null;
  private previousActiveElement: HTMLElement | null = null;

  ngOnInit(): void {
    if (this.appFocusTrap) {
      this.initializeFocusTrap();
    }
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  private initializeFocusTrap(): void {
    // Store the previously focused element
    this.previousActiveElement = document.activeElement as HTMLElement;
    
    // Set up the focus trap
    this.cleanupFocusTrap = this.accessibilityService.trapFocus(this.elementRef.nativeElement);
    
    // Focus the initial element if specified
    if (this.focusTrapInitialFocus) {
      setTimeout(() => {
        this.focusTrapInitialFocus?.focus();
      }, 0);
    }
  }

  private cleanup(): void {
    if (this.cleanupFocusTrap) {
      this.cleanupFocusTrap();
      this.cleanupFocusTrap = null;
    }
    
    // Restore focus to the return element or previous element
    const returnElement = this.focusTrapReturnElement || this.previousActiveElement;
    if (returnElement) {
      this.accessibilityService.restoreFocus(returnElement);
    }
  }
}