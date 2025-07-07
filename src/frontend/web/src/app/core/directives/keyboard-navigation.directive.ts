import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';
import { AccessibilityService } from '../services/accessibility.service';

@Directive({
  selector: '[appKeyboardNavigation]',
  standalone: true
})
export class KeyboardNavigationDirective {
  @Input() appKeyboardNavigation: 'grid' | 'list' | 'tabs' | 'menu' = 'list';
  @Input() keyboardNavigationScope: string = '';
  
  private elementRef = inject(ElementRef);
  private accessibilityService = inject(AccessibilityService);
  private currentIndex = 0;
  
  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const navigableElements = this.getNavigableElements();
    if (navigableElements.length === 0) return;

    switch (this.appKeyboardNavigation) {
      case 'list':
      case 'menu':
        this.handleListNavigation(event, navigableElements);
        break;
      case 'grid':
        this.handleGridNavigation(event, navigableElements);
        break;
      case 'tabs':
        this.handleTabNavigation(event, navigableElements);
        break;
    }
  }

  @HostListener('focusin', ['$event'])
  onFocusIn(event: FocusEvent): void {
    const navigableElements = this.getNavigableElements();
    const targetElement = event.target as HTMLElement;
    
    this.currentIndex = navigableElements.findIndex(el => el === targetElement);
    if (this.currentIndex === -1) {
      this.currentIndex = 0;
    }
  }

  private handleListNavigation(event: KeyboardEvent, elements: HTMLElement[]): void {
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'down'
        );
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'up'
        );
        break;
      case 'Home':
        event.preventDefault();
        this.currentIndex = 0;
        elements[0]?.focus();
        break;
      case 'End':
        event.preventDefault();
        this.currentIndex = elements.length - 1;
        elements[this.currentIndex]?.focus();
        break;
      case 'Enter':
      case ' ':
        if (elements[this.currentIndex]) {
          event.preventDefault();
          this.activateElement(elements[this.currentIndex]);
        }
        break;
    }
  }

  private handleGridNavigation(event: KeyboardEvent, elements: HTMLElement[]): void {
    const gridContainer = this.elementRef.nativeElement;
    const computedStyle = window.getComputedStyle(gridContainer);
    const columns = this.getGridColumns(computedStyle);
    
    switch (event.key) {
      case 'ArrowRight':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'right'
        );
        break;
      case 'ArrowLeft':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'left'
        );
        break;
      case 'ArrowDown':
        event.preventDefault();
        const downIndex = Math.min(this.currentIndex + columns, elements.length - 1);
        this.currentIndex = downIndex;
        elements[downIndex]?.focus();
        break;
      case 'ArrowUp':
        event.preventDefault();
        const upIndex = Math.max(this.currentIndex - columns, 0);
        this.currentIndex = upIndex;
        elements[upIndex]?.focus();
        break;
      case 'Home':
        event.preventDefault();
        this.currentIndex = 0;
        elements[0]?.focus();
        break;
      case 'End':
        event.preventDefault();
        this.currentIndex = elements.length - 1;
        elements[this.currentIndex]?.focus();
        break;
      case 'Enter':
      case ' ':
        if (elements[this.currentIndex]) {
          event.preventDefault();
          this.activateElement(elements[this.currentIndex]);
        }
        break;
    }
  }

  private handleTabNavigation(event: KeyboardEvent, elements: HTMLElement[]): void {
    switch (event.key) {
      case 'ArrowRight':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'right'
        );
        this.setTabSelection(elements);
        break;
      case 'ArrowLeft':
        event.preventDefault();
        this.currentIndex = this.accessibilityService.handleArrowNavigation(
          elements, this.currentIndex, 'left'
        );
        this.setTabSelection(elements);
        break;
      case 'Home':
        event.preventDefault();
        this.currentIndex = 0;
        elements[0]?.focus();
        this.setTabSelection(elements);
        break;
      case 'End':
        event.preventDefault();
        this.currentIndex = elements.length - 1;
        elements[this.currentIndex]?.focus();
        this.setTabSelection(elements);
        break;
    }
  }

  private getNavigableElements(): HTMLElement[] {
    const container = this.elementRef.nativeElement;
    let selector = '[role="button"], button, [role="tab"], [role="menuitem"], [role="option"], [role="gridcell"]';
    
    if (this.keyboardNavigationScope) {
      selector = this.keyboardNavigationScope;
    }
    
    return this.accessibilityService.getFocusableElements(container)
      .filter(el => el.matches(selector) || el.hasAttribute('data-keyboard-nav'));
  }

  private getGridColumns(computedStyle: CSSStyleDeclaration): number {
    const gridTemplateColumns = computedStyle.gridTemplateColumns;
    if (gridTemplateColumns && gridTemplateColumns !== 'none') {
      return gridTemplateColumns.split(' ').length;
    }
    
    // Fallback: try to detect from flexbox
    const flexDirection = computedStyle.flexDirection;
    if (flexDirection === 'row' || flexDirection === 'row-reverse') {
      const children = Array.from(this.elementRef.nativeElement.children) as HTMLElement[];
      const firstChild = children[0];
      if (firstChild) {
        const firstChildRect = firstChild.getBoundingClientRect();
        return children.filter(child => {
          const rect = child.getBoundingClientRect();
          return Math.abs(rect.top - firstChildRect.top) < 5; // Same row
        }).length;
      }
    }
    
    return 1; // Default to single column
  }

  private setTabSelection(elements: HTMLElement[]): void {
    elements.forEach((element, index) => {
      const isSelected = index === this.currentIndex;
      element.setAttribute('aria-selected', isSelected.toString());
      element.setAttribute('tabindex', isSelected ? '0' : '-1');
    });
  }

  private activateElement(element: HTMLElement): void {
    if (element.tagName === 'BUTTON' || element.hasAttribute('role')) {
      element.click();
    } else if (element.tagName === 'A') {
      (element as HTMLAnchorElement).click();
    }
  }
}