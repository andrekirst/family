import { TestBed } from '@angular/core/testing';
import { I18nPipe } from './i18n.pipe';
import { I18nService } from '../../core/services/i18n.service';
import { LocalizationService } from '../../core/services/localization.service';
import { LOCALE_ID } from '@angular/core';

describe('I18nPipe', () => {
  let pipe: I18nPipe;
  let i18nService: I18nService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        I18nPipe,
        I18nService,
        LocalizationService,
        { provide: LOCALE_ID, useValue: 'de' }
      ]
    });
    pipe = TestBed.inject(I18nPipe);
    i18nService = TestBed.inject(I18nService);
  });

  it('create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should transform translation keys', () => {
    spyOn(i18nService, 'translate').and.returnValue('Translated Text');
    
    const result = pipe.transform('test.key');
    expect(i18nService.translate).toHaveBeenCalledWith('test.key', undefined);
    expect(result).toBe('Translated Text');
  });

  it('should transform translation keys with parameters', () => {
    const params = { name: 'John' };
    spyOn(i18nService, 'translate').and.returnValue('Hello John');
    
    const result = pipe.transform('test.key', params);
    expect(i18nService.translate).toHaveBeenCalledWith('test.key', params);
    expect(result).toBe('Hello John');
  });

  it('should handle empty or undefined keys', () => {
    spyOn(i18nService, 'translate').and.returnValue('');
    
    const result = pipe.transform('');
    expect(i18nService.translate).toHaveBeenCalledWith('', undefined);
    expect(result).toBe('');
  });

  it('should cleanup subscription on destroy', () => {
    const subscription = pipe['subscription'];
    if (subscription) {
      spyOn(subscription, 'unsubscribe');
      pipe.ngOnDestroy();
      expect(subscription.unsubscribe).toHaveBeenCalled();
    }
  });
});