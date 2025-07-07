import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { By } from '@angular/platform-browser';
import { Component, DebugElement } from '@angular/core';
import { of } from 'rxjs';

import { FamilyCreationModalComponent, FamilyCreationResult } from './family-creation-modal.component';
import { AccessibilityService } from '../../../core/services/accessibility.service';
import { FocusTrapDirective } from '../../../core/directives/focus-trap.directive';

// Mock Host Component for testing
@Component({
  template: `
    <app-family-creation-modal
      [isOpen]="isOpen"
      [isLoading]="isLoading"
      (close)="onClose()"
      (submit)="onSubmit($event)">
    </app-family-creation-modal>
  `
})
class TestHostComponent {
  isOpen = false;
  isLoading = false;
  closeEmitted = false;
  submittedFamilyName: string | null = null;

  onClose(): void {
    this.closeEmitted = true;
  }

  onSubmit(familyName: string): void {
    this.submittedFamilyName = familyName;
  }
}

describe('FamilyCreationModalComponent', () => {
  let component: FamilyCreationModalComponent;
  let hostComponent: TestHostComponent;
  let fixture: ComponentFixture<TestHostComponent>;
  let modalFixture: ComponentFixture<FamilyCreationModalComponent>;
  let translateService: jasmine.SpyObj<TranslateService>;
  let accessibilityService: jasmine.SpyObj<AccessibilityService>;

  const mockTranslations = {
    'family.createModal.title': 'Create Family',
    'family.createModal.description': 'Enter your family name',
    'family.createModal.nameLabel': 'Family Name',
    'family.createModal.namePlaceholder': 'Enter family name',
    'family.createModal.cancel': 'Cancel',
    'family.createModal.createButton': 'Create Family',
    'family.createModal.creating': 'Creating...',
    'family.createModal.close': 'Close',
    'family.createModal.formLabel': 'Family creation form',
    'family.createModal.loadingMessage': 'Creating your family...',
    'family.createModal.nameRequired': 'Family name is required',
    'family.createModal.nameMinLength': 'Family name must be at least 2 characters',
    'family.createModal.nameMaxLength': 'Family name must not exceed 100 characters',
    'family.createModal.namePattern': 'Family name contains invalid characters'
  };

  beforeEach(async () => {
    const translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);
    const accessibilitySpy = jasmine.createSpyObj('AccessibilityService', ['announceToScreenReader', 'trapFocus']);

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        TranslateModule.forRoot(),
        FamilyCreationModalComponent,
        FocusTrapDirective
      ],
      declarations: [TestHostComponent],
      providers: [
        FormBuilder,
        { provide: TranslateService, useValue: translateSpy },
        { provide: AccessibilityService, useValue: accessibilitySpy }
      ]
    }).compileComponents();

    translateService = TestBed.inject(TranslateService) as jasmine.SpyObj<TranslateService>;
    accessibilityService = TestBed.inject(AccessibilityService) as jasmine.SpyObj<AccessibilityService>;

    // Setup translation mocks
    translateService.instant.and.callFake((key: string) => mockTranslations[key as keyof typeof mockTranslations] || key);

    fixture = TestBed.createComponent(TestHostComponent);
    hostComponent = fixture.componentInstance;
    
    // Also create standalone component for direct testing
    modalFixture = TestBed.createComponent(FamilyCreationModalComponent);
    component = modalFixture.componentInstance;

    fixture.detectChanges();
    modalFixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.isOpen).toBeFalse();
      expect(component.isLoading).toBeFalse();
      expect(component.familyForm).toBeDefined();
      expect(component.familyForm.get('familyName')?.value).toBe('');
    });

    it('should setup form with correct validators', () => {
      const familyNameControl = component.familyForm.get('familyName');
      
      expect(familyNameControl?.hasError('required')).toBeTruthy();
      
      familyNameControl?.setValue('a');
      expect(familyNameControl?.hasError('minlength')).toBeTruthy();
      
      familyNameControl?.setValue('a'.repeat(101));
      expect(familyNameControl?.hasError('maxlength')).toBeTruthy();
      
      familyNameControl?.setValue('Invalid123');
      expect(familyNameControl?.hasError('pattern')).toBeTruthy();
      
      familyNameControl?.setValue('Valid Family Name');
      expect(familyNameControl?.valid).toBeTruthy();
    });
  });

  describe('Modal Visibility', () => {
    it('should not render modal when isOpen is false', () => {
      hostComponent.isOpen = false;
      fixture.detectChanges();
      
      const modalElement = fixture.debugElement.query(By.css('.modal-overlay'));
      expect(modalElement).toBeNull();
    });

    it('should render modal when isOpen is true', () => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
      
      const modalElement = fixture.debugElement.query(By.css('.modal-overlay'));
      expect(modalElement).toBeTruthy();
    });

    it('should have correct ARIA attributes when open', () => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
      
      const modalOverlay = fixture.debugElement.query(By.css('.modal-overlay'));
      expect(modalOverlay.nativeElement.getAttribute('role')).toBe('dialog');
      expect(modalOverlay.nativeElement.getAttribute('aria-modal')).toBe('true');
      expect(modalOverlay.nativeElement.getAttribute('aria-labelledby')).toBe('modal-title');
      expect(modalOverlay.nativeElement.getAttribute('aria-describedby')).toBe('modal-description');
    });
  });

  describe('Form Interaction', () => {
    beforeEach(() => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
    });

    it('should display form labels and placeholders', () => {
      const label = fixture.debugElement.query(By.css('label[for="familyName"]'));
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      expect(label.nativeElement.textContent.trim()).toContain('Family Name');
      expect(input.nativeElement.placeholder).toBe('Enter family name');
    });

    it('should validate required field', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      // Touch the field without entering value
      input.nativeElement.focus();
      input.nativeElement.blur();
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('#familyName-error'));
      expect(errorElement?.nativeElement.textContent.trim()).toBe('Family name is required');
    });

    it('should validate minimum length', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      input.nativeElement.value = 'a';
      input.nativeElement.dispatchEvent(new Event('input'));
      input.nativeElement.focus();
      input.nativeElement.blur();
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('#familyName-error'));
      expect(errorElement?.nativeElement.textContent.trim()).toBe('Family name must be at least 2 characters');
    });

    it('should validate pattern', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      input.nativeElement.value = 'Invalid123!@#';
      input.nativeElement.dispatchEvent(new Event('input'));
      input.nativeElement.focus();
      input.nativeElement.blur();
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('#familyName-error'));
      expect(errorElement?.nativeElement.textContent.trim()).toBe('Family name contains invalid characters');
    });

    it('should accept valid family names', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      const validNames = ['Smith', 'Müller-Weber', 'Von der Heide', 'O\'Connor-Schmidt'];
      
      validNames.forEach(name => {
        input.nativeElement.value = name;
        input.nativeElement.dispatchEvent(new Event('input'));
        fixture.detectChanges();
        
        expect(component.familyForm.get('familyName')?.valid).toBeTruthy();
      });
    });
  });

  describe('Button States', () => {
    beforeEach(() => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
    });

    it('should disable submit button when form is invalid', () => {
      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(submitButton.nativeElement.disabled).toBeTruthy();
    });

    it('should enable submit button when form is valid', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      input.nativeElement.value = 'Valid Family Name';
      input.nativeElement.dispatchEvent(new Event('input'));
      fixture.detectChanges();
      
      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(submitButton.nativeElement.disabled).toBeFalsy();
    });

    it('should disable buttons when loading', () => {
      hostComponent.isLoading = true;
      fixture.detectChanges();
      
      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      const cancelButton = fixture.debugElement.query(By.css('button[type="button"]'));
      const closeButton = fixture.debugElement.query(By.css('.modal-close-btn'));
      
      expect(submitButton.nativeElement.disabled).toBeTruthy();
      expect(cancelButton.nativeElement.disabled).toBeTruthy();
      expect(closeButton.nativeElement.disabled).toBeTruthy();
    });

    it('should show loading spinner when loading', () => {
      hostComponent.isLoading = true;
      fixture.detectChanges();
      
      const loadingSpinner = fixture.debugElement.query(By.css('.animate-spin'));
      const loadingText = fixture.debugElement.query(By.css('#loading-text'));
      
      expect(loadingSpinner).toBeTruthy();
      expect(loadingText?.nativeElement.textContent.trim()).toBe('Creating your family...');
    });
  });

  describe('Event Handling', () => {
    beforeEach(() => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
    });

    it('should emit close event when cancel button is clicked', () => {
      const cancelButton = fixture.debugElement.query(By.css('button[type="button"]'));
      
      cancelButton.nativeElement.click();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeTruthy();
    });

    it('should emit close event when close button is clicked', () => {
      const closeButton = fixture.debugElement.query(By.css('.modal-close-btn'));
      
      closeButton.nativeElement.click();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeTruthy();
    });

    it('should emit close event when overlay is clicked', () => {
      const overlay = fixture.debugElement.query(By.css('.modal-overlay'));
      
      overlay.nativeElement.click();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeTruthy();
    });

    it('should not close when modal container is clicked', () => {
      const container = fixture.debugElement.query(By.css('.modal-container'));
      
      container.nativeElement.click();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeFalsy();
    });

    it('should emit submit event with family name on form submit', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      const form = fixture.debugElement.query(By.css('form'));
      
      input.nativeElement.value = 'Test Family';
      input.nativeElement.dispatchEvent(new Event('input'));
      fixture.detectChanges();
      
      form.nativeElement.dispatchEvent(new Event('submit'));
      fixture.detectChanges();
      
      expect(hostComponent.submittedFamilyName).toBe('Test Family');
    });

    it('should not submit when form is invalid', () => {
      const form = fixture.debugElement.query(By.css('form'));
      
      form.nativeElement.dispatchEvent(new Event('submit'));
      fixture.detectChanges();
      
      expect(hostComponent.submittedFamilyName).toBeNull();
    });
  });

  describe('Keyboard Navigation', () => {
    beforeEach(() => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
    });

    it('should close modal on Escape key', fakeAsync(() => {
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);
      tick();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeTruthy();
    }));

    it('should not close modal on Escape when loading', fakeAsync(() => {
      hostComponent.isLoading = true;
      fixture.detectChanges();
      
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      document.dispatchEvent(escapeEvent);
      tick();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeFalsy();
    }));

    it('should not close modal on other keys', fakeAsync(() => {
      const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
      document.dispatchEvent(enterEvent);
      tick();
      fixture.detectChanges();
      
      expect(hostComponent.closeEmitted).toBeFalsy();
    }));
  });

  describe('Focus Management', () => {
    it('should focus on family name input when modal opens', fakeAsync(() => {
      spyOn(document, 'getElementById').and.returnValue({
        focus: jasmine.createSpy('focus')
      } as any);
      
      component.isOpen = true;
      component.ngOnInit();
      tick(100);
      
      expect(document.getElementById).toHaveBeenCalledWith('familyName');
    }));

    it('should trap focus within modal', () => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
      
      const modalContainer = fixture.debugElement.query(By.css('.modal-container'));
      expect(modalContainer.nativeElement.getAttribute('appFocusTrap')).toBeDefined();
    });
  });

  describe('Form Reset', () => {
    it('should reset form when modal closes', () => {
      component.familyForm.get('familyName')?.setValue('Test Name');
      component.familyForm.get('familyName')?.markAsTouched();
      
      component.onClose();
      
      expect(component.familyForm.get('familyName')?.value).toBe('');
      expect(component.familyForm.get('familyName')?.touched).toBeFalsy();
    });
  });

  describe('Error Messages', () => {
    it('should return correct error messages for different validation errors', () => {
      const familyNameControl = component.familyForm.get('familyName');
      
      // Required error
      familyNameControl?.setErrors({ required: true });
      familyNameControl?.markAsTouched();
      expect(component.errorMessage).toBe('Family name is required');
      
      // Minlength error
      familyNameControl?.setErrors({ minlength: { requiredLength: 2, actualLength: 1 } });
      expect(component.errorMessage).toBe('Family name must be at least 2 characters');
      
      // Maxlength error
      familyNameControl?.setErrors({ maxlength: { requiredLength: 100, actualLength: 101 } });
      expect(component.errorMessage).toBe('Family name must not exceed 100 characters');
      
      // Pattern error
      familyNameControl?.setErrors({ pattern: { requiredPattern: '', actualValue: '' } });
      expect(component.errorMessage).toBe('Family name contains invalid characters');
    });

    it('should show error with correct ARIA attributes', () => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
      
      const input = fixture.debugElement.query(By.css('#familyName'));
      input.nativeElement.focus();
      input.nativeElement.blur();
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('#familyName-error'));
      expect(errorElement.nativeElement.getAttribute('role')).toBe('alert');
      expect(errorElement.nativeElement.getAttribute('aria-live')).toBe('polite');
      
      expect(input.nativeElement.getAttribute('aria-describedby')).toBe('familyName-error');
      expect(input.nativeElement.getAttribute('aria-invalid')).toBe('true');
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      hostComponent.isOpen = true;
      fixture.detectChanges();
    });

    it('should have proper ARIA structure', () => {
      const title = fixture.debugElement.query(By.css('#modal-title'));
      const description = fixture.debugElement.query(By.css('#modal-description'));
      const form = fixture.debugElement.query(By.css('form'));
      
      expect(title).toBeTruthy();
      expect(description).toBeTruthy();
      expect(form.nativeElement.getAttribute('role')).toBe('form');
    });

    it('should have required field marked properly', () => {
      const input = fixture.debugElement.query(By.css('#familyName'));
      
      expect(input.nativeElement.required).toBeTruthy();
      expect(input.nativeElement.getAttribute('aria-required')).toBe('true');
    });

    it('should have loading state accessible to screen readers', () => {
      hostComponent.isLoading = true;
      fixture.detectChanges();
      
      const loadingText = fixture.debugElement.query(By.css('#loading-text'));
      const submitButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      
      expect(loadingText?.nativeElement.getAttribute('aria-live')).toBe('polite');
      expect(loadingText?.nativeElement.classList.contains('sr-only')).toBeTruthy();
      expect(submitButton.nativeElement.getAttribute('aria-describedby')).toBe('loading-text');
    });
  });

  describe('Cleanup', () => {
    it('should remove event listener on destroy', () => {
      spyOn(document, 'removeEventListener');
      
      component.ngOnDestroy();
      
      expect(document.removeEventListener).toHaveBeenCalledWith('keydown', jasmine.any(Function));
    });

    it('should complete destroy subject on destroy', () => {
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');
      
      component.ngOnDestroy();
      
      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });
});