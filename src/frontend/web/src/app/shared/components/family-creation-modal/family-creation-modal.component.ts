import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, inject, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AccessibilityService } from '../../../core/services/accessibility.service';
import { FocusTrapDirective } from '../../../core/directives/focus-trap.directive';

export interface FamilyCreationResult {
  success: boolean;
  familyName?: string;
  errorMessage?: string;
}

@Component({
  selector: 'app-family-creation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, FocusTrapDirective],
  templateUrl: './family-creation-modal.component.html',
  styleUrls: ['./family-creation-modal.component.scss']
})
export class FamilyCreationModalComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() isLoading = false;
  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<string>();

  familyForm: FormGroup;
  private destroy$ = new Subject<void>();
  private translate = inject(TranslateService);
  private accessibilityService = inject(AccessibilityService);
  private elementRef = inject(ElementRef);

  constructor(private fb: FormBuilder) {
    this.familyForm = this.fb.group({
      familyName: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        Validators.pattern(/^[a-zA-ZäöüÄÖÜß\s-]+$/)
      ]]
    });
  }

  ngOnInit(): void {
    // Focus on family name input when modal opens
    if (this.isOpen) {
      setTimeout(() => {
        const input = document.getElementById('familyName');
        input?.focus();
      }, 100);
    }

    // Listen for escape key to close modal
    document.addEventListener('keydown', this.handleEscapeKey.bind(this));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    document.removeEventListener('keydown', this.handleEscapeKey.bind(this));
  }

  private handleEscapeKey(event: KeyboardEvent): void {
    if (event.key === 'Escape' && this.isOpen) {
      this.onClose();
    }
  }

  onClose(): void {
    if (!this.isLoading) {
      this.close.emit();
      this.resetForm();
    }
  }

  onSubmit(): void {
    if (this.familyForm.valid && !this.isLoading) {
      const familyName = this.familyForm.get('familyName')?.value?.trim();
      if (familyName) {
        this.submit.emit(familyName);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  private resetForm(): void {
    this.familyForm.reset();
    this.familyForm.markAsUntouched();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.familyForm.controls).forEach(key => {
      const control = this.familyForm.get(key);
      control?.markAsTouched();
    });
  }

  get familyNameControl() {
    return this.familyForm.get('familyName');
  }

  get hasError(): boolean {
    return this.familyNameControl?.invalid && this.familyNameControl?.touched || false;
  }

  get errorMessage(): string {
    if (this.familyNameControl?.errors) {
      if (this.familyNameControl.errors['required']) {
        return this.translate.instant('family.createModal.nameRequired');
      }
      if (this.familyNameControl.errors['minlength']) {
        return this.translate.instant('family.createModal.nameMinLength');
      }
      if (this.familyNameControl.errors['maxlength']) {
        return this.translate.instant('family.createModal.nameMaxLength');
      }
      if (this.familyNameControl.errors['pattern']) {
        return this.translate.instant('family.createModal.namePattern');
      }
    }
    return '';
  }
}