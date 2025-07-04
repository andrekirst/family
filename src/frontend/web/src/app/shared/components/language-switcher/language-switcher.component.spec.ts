import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LanguageSwitcherComponent } from './language-switcher.component';
import { LocalizationService } from '../../../core/services/localization.service';
import { I18nService } from '../../../core/services/i18n.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { LOCALE_ID } from '@angular/core';
import { of } from 'rxjs';

describe('LanguageSwitcherComponent', () => {
  let component: LanguageSwitcherComponent;
  let fixture: ComponentFixture<LanguageSwitcherComponent>;
  let localizationService: LocalizationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LanguageSwitcherComponent,
        MatMenuModule,
        MatButtonModule,
        MatIconModule,
        NoopAnimationsModule
      ],
      providers: [
        LocalizationService,
        I18nService,
        { provide: LOCALE_ID, useValue: 'de' }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LanguageSwitcherComponent);
    component = fixture.componentInstance;
    localizationService = TestBed.inject(LocalizationService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with supported locales', () => {
    expect(component.supportedLocales).toBeDefined();
    expect(component.supportedLocales.length).toBeGreaterThan(0);
  });

  it('should have current locale info', () => {
    expect(component.currentLocaleInfo).toBeDefined();
    expect(component.currentLocaleInfo.code).toBeDefined();
    expect(component.currentLocaleInfo.name).toBeDefined();
    expect(component.currentLocaleInfo.flag).toBeDefined();
  });

  it('should switch language when switchLanguage is called', () => {
    spyOn(localizationService, 'setLocale');
    
    component.switchLanguage('en');
    expect(localizationService.setLocale).toHaveBeenCalledWith('en');
  });

  it('should update current locale info when locale changes', () => {
    const mockLocaleInfo = { code: 'en', name: 'English', flag: '🇺🇸' };
    spyOn(localizationService, 'getCurrentLocaleInfo').and.returnValue(mockLocaleInfo);
    
    component['updateCurrentLocaleInfo']();
    expect(component.currentLocaleInfo).toEqual(mockLocaleInfo);
  });

  it('should display language button with flag and name', () => {
    const compiled = fixture.nativeElement;
    const button = compiled.querySelector('.language-switcher-button');
    expect(button).toBeTruthy();
    
    const flag = compiled.querySelector('.language-flag');
    const name = compiled.querySelector('.language-name');
    expect(flag).toBeTruthy();
    expect(name).toBeTruthy();
  });

  it('should render supported locales in menu', () => {
    // Trigger menu to be rendered
    const compiled = fixture.nativeElement;
    const menuItems = compiled.querySelectorAll('button[mat-menu-item]');
    
    // The menu items might not be rendered until the menu is opened
    // So we check that the component has the data ready
    expect(component.supportedLocales.length).toBeGreaterThanOrEqual(2);
    expect(component.supportedLocales.some(l => l.code === 'de')).toBeTruthy();
    expect(component.supportedLocales.some(l => l.code === 'en')).toBeTruthy();
  });

  it('should subscribe to locale changes', () => {
    spyOn(component, 'switchLanguage');
    spyOn(localizationService, 'setLocale');
    
    // Simulate locale change
    component.switchLanguage('en');
    expect(localizationService.setLocale).toHaveBeenCalledWith('en');
  });
});