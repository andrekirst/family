import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

export interface FamilyCreationResult {
  success: boolean;
  familyName?: string;
  errorMessage?: string;
}

@Component({
  selector: 'app-family-creation-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
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
        return 'Familienname ist erforderlich';
      }
      if (this.familyNameControl.errors['minlength']) {
        return 'Familienname muss mindestens 2 Zeichen lang sein';
      }
      if (this.familyNameControl.errors['maxlength']) {
        return 'Familienname darf maximal 100 Zeichen lang sein';
      }
      if (this.familyNameControl.errors['pattern']) {
        return 'Familienname darf nur Buchstaben, Leerzeichen und Bindestriche enthalten';
      }
    }
    return '';
  }
}