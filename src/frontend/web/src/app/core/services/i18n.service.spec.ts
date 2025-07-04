import { TestBed } from '@angular/core/testing';
import { I18nService } from './i18n.service';
import { LocalizationService } from './localization.service';

describe('I18nService', () => {
  let service: I18nService;
  let localizationService: LocalizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [I18nService, LocalizationService]
    });
    service = TestBed.inject(I18nService);
    localizationService = TestBed.inject(LocalizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should translate a simple key', () => {
    const translatedText = service.translate('app.title');
    expect(translatedText).toBeDefined();
    expect(typeof translatedText).toBe('string');
  });

  it('should return the key if translation is not found', () => {
    const nonExistentKey = 'nonexistent.key';
    const result = service.translate(nonExistentKey);
    expect(result).toBe(nonExistentKey);
  });

  it('should handle parameter replacement', () => {
    // Mock a translation with parameters
    const mockTranslation = 'Hello {name}, welcome to {app}!';
    spyOn(service, 'translate').and.returnValue(mockTranslation);
    
    const result = service.translate('welcome.message', { name: 'John', app: 'Family' });
    expect(result).toBe('Hello John, welcome to Family!');
  });

  it('should provide instant translation', () => {
    const result = service.instant('app.title');
    expect(result).toBeDefined();
    expect(typeof result).toBe('string');
  });

  it('should have translations observable', () => {
    expect(service.translations$).toBeDefined();
    service.translations$.subscribe(translations => {
      expect(translations).toBeDefined();
      expect(typeof translations).toBe('object');
    });
  });
});