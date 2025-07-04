import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LocalizationService } from './localization.service';

export interface TranslationMap {
  [key: string]: string;
}

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  private translationsSubject = new BehaviorSubject<TranslationMap>({});
  private currentTranslations: TranslationMap = {};

  constructor(private localizationService: LocalizationService) {
    this.loadTranslations();
    
    // Subscribe to locale changes
    this.localizationService.currentLocale$.subscribe(locale => {
      this.loadTranslations(locale);
    });
  }

  get translations$(): Observable<TranslationMap> {
    return this.translationsSubject.asObservable();
  }

  translate(key: string, params?: { [key: string]: string | number }): string {
    let translation = this.currentTranslations[key] || key;
    
    // Replace parameters in translation
    if (params) {
      Object.keys(params).forEach(param => {
        const placeholder = `{${param}}`;
        translation = translation.replace(new RegExp(placeholder, 'g'), params[param].toString());
      });
    }
    
    return translation;
  }

  instant(key: string, params?: { [key: string]: string | number }): string {
    return this.translate(key, params);
  }

  private async loadTranslations(locale?: string): Promise<void> {
    const currentLocale = locale || this.localizationService.currentLocale;
    
    try {
      const translationModule = await this.importTranslations(currentLocale);
      this.currentTranslations = translationModule.translations || {};
      this.translationsSubject.next(this.currentTranslations);
    } catch (error) {
      console.error(`Failed to load translations for locale ${currentLocale}:`, error);
      
      // Fallback to German if English fails, or empty object if German fails
      if (currentLocale !== 'de') {
        try {
          const fallbackModule = await this.importTranslations('de');
          this.currentTranslations = fallbackModule.translations || {};
          this.translationsSubject.next(this.currentTranslations);
        } catch (fallbackError) {
          console.error('Failed to load fallback translations:', fallbackError);
          this.currentTranslations = {};
          this.translationsSubject.next(this.currentTranslations);
        }
      }
    }
  }

  private async importTranslations(locale: string): Promise<any> {
    switch (locale) {
      case 'de':
        return import('../../../locale/messages.de.json');
      case 'en':
        return import('../../../locale/messages.en.json');
      default:
        return import('../../../locale/messages.de.json');
    }
  }
}