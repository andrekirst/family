import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { DebugElement } from '@angular/core';

import { FirstTimeUserComponent } from './first-time-user.component';
import { FamilyService, FirstTimeUserInfo, CreateFamilyResult } from '../../../core/services/family.service';
import { FamilyCreationModalComponent } from '../../../shared/components/family-creation-modal/family-creation-modal.component';

describe('FirstTimeUserComponent', () => {
  let component: FirstTimeUserComponent;
  let fixture: ComponentFixture<FirstTimeUserComponent>;
  let familyService: jasmine.SpyObj<FamilyService>;
  let router: jasmine.SpyObj<Router>;
  let translateService: jasmine.SpyObj<TranslateService>;

  const mockFirstTimeUserInfo: FirstTimeUserInfo = {
    isFirstTime: true,
    hasFamily: false,
    userName: 'John Doe',
    email: 'john@example.com'
  };

  const mockExistingUserInfo: FirstTimeUserInfo = {
    isFirstTime: false,
    hasFamily: true,
    userName: 'Jane Doe',
    email: 'jane@example.com'
  };

  const mockTranslations = {
    'family.firstTime.errorDefault': 'An error occurred while checking your status',
    'family.firstTime.errorCreateFamily': 'Failed to create family',
    'family.firstTime.errorCreateFamilyUnexpected': 'An unexpected error occurred while creating your family'
  };

  beforeEach(async () => {
    const familyServiceSpy = jasmine.createSpyObj('FamilyService', ['getFirstTimeUserInfo', 'createFamily']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);

    await TestBed.configureTestingModule({
      imports: [
        TranslateModule.forRoot(),
        FirstTimeUserComponent,
        FamilyCreationModalComponent
      ],
      providers: [
        { provide: FamilyService, useValue: familyServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: TranslateService, useValue: translateSpy }
      ]
    }).compileComponents();

    familyService = TestBed.inject(FamilyService) as jasmine.SpyObj<FamilyService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    translateService = TestBed.inject(TranslateService) as jasmine.SpyObj<TranslateService>;

    // Setup translation mocks
    translateService.instant.and.callFake((key: string) => mockTranslations[key as keyof typeof mockTranslations] || key);

    fixture = TestBed.createComponent(FirstTimeUserComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should start with loading state', () => {
      expect(component.isLoading).toBeTruthy();
      expect(component.showFamilyModal).toBeFalsy();
      expect(component.isCreatingFamily).toBeFalsy();
      expect(component.userInfo).toBeNull();
      expect(component.errorMessage).toBeNull();
    });

    it('should call checkUserStatus on init', () => {
      spyOn(component as any, 'checkUserStatus');
      
      component.ngOnInit();
      
      expect(component['checkUserStatus']).toHaveBeenCalled();
    });
  });

  describe('User Status Check', () => {
    it('should load first-time user info successfully', fakeAsync(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      
      fixture.detectChanges(); // This triggers ngOnInit
      tick();
      
      expect(component.isLoading).toBeFalsy();
      expect(component.userInfo).toEqual(mockFirstTimeUserInfo);
      expect(component.errorMessage).toBeNull();
      expect(router.navigate).not.toHaveBeenCalled();
    }));

    it('should redirect existing user with family to dashboard', fakeAsync(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockExistingUserInfo));
      
      fixture.detectChanges();
      tick();
      
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    }));

    it('should handle error when checking user status', fakeAsync(() => {
      const error = new Error('Network error');
      familyService.getFirstTimeUserInfo.and.returnValue(throwError(() => error));
      
      fixture.detectChanges();
      tick();
      
      expect(component.isLoading).toBeFalsy();
      expect(component.errorMessage).toBe('An error occurred while checking your status');
      expect(component.userInfo).toBeNull();
    }));
  });

  describe('Family Creation Modal', () => {
    beforeEach(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
    });

    it('should show family modal when create family is clicked', () => {
      component.onCreateFamily();
      
      expect(component.showFamilyModal).toBeTruthy();
    });

    it('should hide family modal when modal close is called', () => {
      component.showFamilyModal = true;
      
      component.onModalClose();
      
      expect(component.showFamilyModal).toBeFalsy();
    });

    it('should pass correct props to family modal', () => {
      component.showFamilyModal = true;
      component.isCreatingFamily = true;
      fixture.detectChanges();
      
      const modalComponent = fixture.debugElement.query(By.directive(FamilyCreationModalComponent));
      
      expect(modalComponent.componentInstance.isOpen).toBeTruthy();
      expect(modalComponent.componentInstance.isLoading).toBeTruthy();
    });

    it('should handle modal events correctly', () => {
      component.showFamilyModal = true;
      fixture.detectChanges();
      
      const modalComponent = fixture.debugElement.query(By.directive(FamilyCreationModalComponent));
      
      // Test close event
      spyOn(component, 'onModalClose');
      modalComponent.componentInstance.close.emit();
      expect(component.onModalClose).toHaveBeenCalled();
      
      // Test submit event
      spyOn(component, 'onFamilySubmit');
      modalComponent.componentInstance.submit.emit('Test Family');
      expect(component.onFamilySubmit).toHaveBeenCalledWith('Test Family');
    });
  });

  describe('Family Creation', () => {
    beforeEach(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
    });

    it('should create family successfully', fakeAsync(() => {
      const successResult: CreateFamilyResult = {
        success: true,
        family: {
          id: '123',
          name: 'Test Family',
          ownerId: 'user123',
          members: []
        }
      };
      
      familyService.createFamily.and.returnValue(of(successResult));
      component.showFamilyModal = true;
      
      component.onFamilySubmit('Test Family');
      tick();
      
      expect(component.isCreatingFamily).toBeFalsy();
      expect(component.showFamilyModal).toBeFalsy();
      expect(component.errorMessage).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard'], {
        queryParams: { familyCreated: 'true' }
      });
    }));

    it('should handle family creation failure', fakeAsync(() => {
      const failureResult: CreateFamilyResult = {
        success: false,
        errorMessage: 'Family name already exists'
      };
      
      familyService.createFamily.and.returnValue(of(failureResult));
      
      component.onFamilySubmit('Test Family');
      tick();
      
      expect(component.isCreatingFamily).toBeFalsy();
      expect(component.showFamilyModal).toBeTruthy(); // Modal stays open
      expect(component.errorMessage).toBe('Family name already exists');
    }));

    it('should handle validation errors', fakeAsync(() => {
      const validationResult: CreateFamilyResult = {
        success: false,
        validationErrors: ['Name is required', 'Name too short']
      };
      
      familyService.createFamily.and.returnValue(of(validationResult));
      
      component.onFamilySubmit('Test Family');
      tick();
      
      expect(component.errorMessage).toBe('Name is required, Name too short');
    }));

    it('should handle unexpected errors during family creation', fakeAsync(() => {
      const error = new Error('Server error');
      familyService.createFamily.and.returnValue(throwError(() => error));
      
      component.onFamilySubmit('Test Family');
      tick();
      
      expect(component.isCreatingFamily).toBeFalsy();
      expect(component.errorMessage).toBe('An unexpected error occurred while creating your family');
    }));

    it('should set loading state during family creation', () => {
      familyService.createFamily.and.returnValue(of({ success: true }));
      
      component.onFamilySubmit('Test Family');
      
      expect(component.isCreatingFamily).toBeTruthy();
      expect(component.errorMessage).toBeNull();
    });
  });

  describe('Navigation', () => {
    beforeEach(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
    });

    it('should navigate to dashboard when skip is clicked', () => {
      component.onSkipForNow();
      
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });

  describe('Error Handling and Retry', () => {
    it('should allow retry after error', () => {
      spyOn(component as any, 'checkUserStatus');
      
      component.onRetry();
      
      expect(component['checkUserStatus']).toHaveBeenCalled();
    });

    it('should clear error message on retry', () => {
      component.errorMessage = 'Previous error';
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      
      component.onRetry();
      
      expect(component.errorMessage).toBeNull();
    });
  });

  describe('Component Cleanup', () => {
    it('should complete destroy subject on destroy', () => {
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');
      
      component.ngOnDestroy();
      
      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });

  describe('Template Rendering', () => {
    it('should show loading state initially', () => {
      component.isLoading = true;
      fixture.detectChanges();
      
      const loadingElement = fixture.debugElement.query(By.css('[data-testid="loading"]'));
      expect(loadingElement).toBeTruthy();
    });

    it('should show first-time user content when loaded', () => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
      
      const contentElement = fixture.debugElement.query(By.css('[data-testid="first-time-content"]'));
      expect(contentElement).toBeTruthy();
    });

    it('should show error state when error occurs', () => {
      component.isLoading = false;
      component.errorMessage = 'Test error';
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('[data-testid="error-state"]'));
      expect(errorElement).toBeTruthy();
    });

    it('should show family creation modal when open', () => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      component.showFamilyModal = true;
      fixture.detectChanges();
      
      const modalElement = fixture.debugElement.query(By.directive(FamilyCreationModalComponent));
      expect(modalElement).toBeTruthy();
    });

    it('should show user information when available', () => {
      component.isLoading = false;
      component.userInfo = mockFirstTimeUserInfo;
      fixture.detectChanges();
      
      // Check if user name and email are displayed (implementation depends on template)
      const welcomeText = fixture.debugElement.query(By.css('[data-testid="welcome-message"]'));
      expect(welcomeText?.nativeElement.textContent).toContain(mockFirstTimeUserInfo.userName);
    });
  });

  describe('Button Interactions', () => {
    beforeEach(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
    });

    it('should call onCreateFamily when create family button is clicked', () => {
      spyOn(component, 'onCreateFamily');
      
      const createButton = fixture.debugElement.query(By.css('[data-testid="create-family-btn"]'));
      createButton?.nativeElement.click();
      
      expect(component.onCreateFamily).toHaveBeenCalled();
    });

    it('should call onSkipForNow when skip button is clicked', () => {
      spyOn(component, 'onSkipForNow');
      
      const skipButton = fixture.debugElement.query(By.css('[data-testid="skip-btn"]'));
      skipButton?.nativeElement.click();
      
      expect(component.onSkipForNow).toHaveBeenCalled();
    });

    it('should call onRetry when retry button is clicked', () => {
      component.errorMessage = 'Test error';
      fixture.detectChanges();
      
      spyOn(component, 'onRetry');
      
      const retryButton = fixture.debugElement.query(By.css('[data-testid="retry-btn"]'));
      retryButton?.nativeElement.click();
      
      expect(component.onRetry).toHaveBeenCalled();
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
    });

    it('should have proper heading structure', () => {
      const headings = fixture.debugElement.queryAll(By.css('h1, h2, h3, h4, h5, h6'));
      expect(headings.length).toBeGreaterThan(0);
    });

    it('should have accessible error messages', () => {
      component.errorMessage = 'Test error';
      fixture.detectChanges();
      
      const errorElement = fixture.debugElement.query(By.css('[role="alert"]'));
      expect(errorElement).toBeTruthy();
    });

    it('should have proper button labels', () => {
      const buttons = fixture.debugElement.queryAll(By.css('button'));
      
      buttons.forEach(button => {
        const hasLabel = button.nativeElement.textContent.trim() || 
                        button.nativeElement.getAttribute('aria-label');
        expect(hasLabel).toBeTruthy();
      });
    });
  });

  describe('State Management', () => {
    it('should maintain correct state during user status check flow', fakeAsync(() => {
      // Initial state
      expect(component.isLoading).toBeTruthy();
      
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      
      fixture.detectChanges();
      tick();
      
      // After successful load
      expect(component.isLoading).toBeFalsy();
      expect(component.userInfo).toEqual(mockFirstTimeUserInfo);
      expect(component.errorMessage).toBeNull();
    }));

    it('should maintain correct state during family creation flow', fakeAsync(() => {
      familyService.getFirstTimeUserInfo.and.returnValue(of(mockFirstTimeUserInfo));
      fixture.detectChanges();
      
      // Start family creation
      component.onCreateFamily();
      expect(component.showFamilyModal).toBeTruthy();
      
      // Submit family creation
      familyService.createFamily.and.returnValue(of({ success: true }));
      component.onFamilySubmit('Test Family');
      
      expect(component.isCreatingFamily).toBeTruthy();
      
      tick();
      
      expect(component.isCreatingFamily).toBeFalsy();
      expect(component.showFamilyModal).toBeFalsy();
    }));
  });
});