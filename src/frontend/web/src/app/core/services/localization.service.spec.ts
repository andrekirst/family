import { TestBed } from '@angular/core/testing';
import { LOCALE_ID } from '@angular/core';
import { LocalizationService } from './localization.service';

describe('LocalizationService', () => {
  let service: LocalizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LocalizationService,
        { provide: LOCALE_ID, useValue: 'de' }
      ]
    });
    service = TestBed.inject(LocalizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have German as default locale', () => {
    expect(service.currentLocale).toBe('de');
  });

  it('should have supported locales', () => {
    expect(service.supportedLocales).toBeDefined();
    expect(service.supportedLocales.length).toBeGreaterThan(0);
    expect(service.supportedLocales.some(locale => locale.code === 'de')).toBeTruthy();
    expect(service.supportedLocales.some(locale => locale.code === 'en')).toBeTruthy();
  });

  it('should provide current locale observable', () => {
    expect(service.currentLocale$).toBeDefined();
    service.currentLocale$.subscribe(locale => {
      expect(locale).toBeDefined();
      expect(typeof locale).toBe('string');
    });
  });

  it('should detect if locale is current locale', () => {
    expect(service.isCurrentLocale('de')).toBeTruthy();
    expect(service.isCurrentLocale('en')).toBeFalsy();
  });

  it('should get current locale info', () => {
    const localeInfo = service.getCurrentLocaleInfo();
    expect(localeInfo).toBeDefined();
    expect(localeInfo.code).toBe('de');
    expect(localeInfo.name).toBeDefined();
    expect(localeInfo.flag).toBeDefined();
  });

  it('should validate supported locales', () => {
    expect(service.supportedLocales.every(locale => 
      locale.code && locale.name && locale.flag
    )).toBeTruthy();
  });

  it('should set locale if supported', () => {
    const consoleSpy = spyOn(console, 'log');
    const initialLocale = service.currentLocale;
    
    // Mock window.location.reload to prevent actual page reload in tests
    spyOn(window.location, 'reload');
    
    service.setLocale('en');
    expect(window.location.reload).toHaveBeenCalled();
  });

  it('should not set locale if not supported', () => {
    const initialLocale = service.currentLocale;
    const reloadSpy = spyOn(window.location, 'reload');
    
    service.setLocale('fr'); // Unsupported locale
    expect(reloadSpy).not.toHaveBeenCalled();
    expect(service.currentLocale).toBe(initialLocale);
  });
});