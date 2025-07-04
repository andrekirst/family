import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';

import { FamilyService, FirstTimeUserInfo, CreateFamilyResult } from '../../../core/services/family.service';
import { FamilyCreationModalComponent } from '../../../shared/components/family-creation-modal/family-creation-modal.component';

@Component({
  selector: 'app-first-time-user',
  standalone: true,
  imports: [CommonModule, FamilyCreationModalComponent],
  templateUrl: './first-time-user.component.html',
  styleUrls: ['./first-time-user.component.scss']
})
export class FirstTimeUserComponent implements OnInit, OnDestroy {
  
  isLoading = true;
  showFamilyModal = false;
  isCreatingFamily = false;
  userInfo: FirstTimeUserInfo | null = null;
  errorMessage: string | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private familyService: FamilyService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.checkUserStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private checkUserStatus(): void {
    this.isLoading = true;
    this.errorMessage = null;
    
    this.familyService.getFirstTimeUserInfo()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (userInfo) => {
          this.userInfo = userInfo;
          
          // If user already has a family, redirect to dashboard
          if (!userInfo.isFirstTime && userInfo.hasFamily) {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (error) => {
          console.error('Error checking user status:', error);
          this.errorMessage = 'Fehler beim Laden der Benutzerinformationen. Bitte versuchen Sie es erneut.';
        }
      });
  }

  onCreateFamily(): void {
    this.showFamilyModal = true;
  }

  onSkipForNow(): void {
    // Navigate to dashboard without creating a family
    this.router.navigate(['/dashboard']);
  }

  onModalClose(): void {
    this.showFamilyModal = false;
  }

  onFamilySubmit(familyName: string): void {
    this.isCreatingFamily = true;
    this.errorMessage = null;
    
    this.familyService.createFamily(familyName)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isCreatingFamily = false)
      )
      .subscribe({
        next: (result: CreateFamilyResult) => {
          if (result.success) {
            // Family created successfully
            this.showFamilyModal = false;
            this.router.navigate(['/dashboard'], {
              queryParams: { familyCreated: 'true' }
            });
          } else {
            // Handle creation error
            this.errorMessage = result.errorMessage || 'Fehler beim Erstellen der Familie';
            if (result.validationErrors && result.validationErrors.length > 0) {
              this.errorMessage = result.validationErrors.join(', ');
            }
          }
        },
        error: (error) => {
          console.error('Error creating family:', error);
          this.errorMessage = 'Unerwarteter Fehler beim Erstellen der Familie. Bitte versuchen Sie es erneut.';
        }
      });
  }

  onRetry(): void {
    this.checkUserStatus();
  }
}