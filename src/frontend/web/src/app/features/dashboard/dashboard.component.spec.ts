import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { AuthService } from '../../core/auth/auth.service';
import { User } from '../../core/graphql/types';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let authService: jasmine.SpyObj<AuthService>;

  const mockUser: User = {
    id: '1',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    fullName: 'Test User',
    preferredLanguage: 'de',
    isActive: true,
    createdAt: '2023-01-01T00:00:00Z',
    lastLoginAt: '2023-01-01T00:00:00Z'
  };

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['loadCurrentUser'], {
      currentUser$: of(mockUser)
    });

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with current user observable', () => {
    fixture.detectChanges();
    
    component.currentUser$.subscribe(user => {
      expect(user).toEqual(mockUser);
    });
  });

  it('should display welcome message with user name', () => {
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Willkommen, Test!');
  });

  it('should display dashboard cards', () => {
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Termine');
    expect(compiled.textContent).toContain('Schule');
    expect(compiled.textContent).toContain('Kindergarten');
    expect(compiled.textContent).toContain('Gesundheit');
    expect(compiled.textContent).toContain('Familie');
    expect(compiled.textContent).toContain('Einstellungen');
  });

  it('should display statistics section', () => {
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Schnellübersicht');
    expect(compiled.textContent).toContain('Termine heute');
    expect(compiled.textContent).toContain('Offene Aufgaben');
    expect(compiled.textContent).toContain('Erinnerungen');
    expect(compiled.textContent).toContain('Familienmitglieder');
  });

  it('should handle user without firstName', () => {
    const userWithoutFirstName: User = {
      ...mockUser,
      firstName: undefined
    };
    
    // Create a new spy with the updated observable
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['loadCurrentUser'], {
      currentUser$: of(userWithoutFirstName)
    });
    
    TestBed.overrideProvider(AuthService, { useValue: authServiceSpy });
    
    const newFixture = TestBed.createComponent(DashboardComponent);
    const newComponent = newFixture.componentInstance;
    newFixture.detectChanges();
    
    const compiled = newFixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Willkommen, Benutzer!');
  });
});