import { Injectable, LOCALE_ID, Inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface SupportedLocale {
  code: string;
  name: string;
  flag: string;
}

@Injectable({
  providedIn: 'root'
})
export class LocalizationService {
  private readonly storageKey = 'family-app-locale';
  private currentLocaleSubject = new BehaviorSubject<string>('de');
  
  readonly supportedLocales: SupportedLocale[] = [
    { code: 'de', name: 'Deutsch', flag: '🇩🇪' },
    { code: 'en', name: 'English', flag: '🇺🇸' }
  ];

  constructor(@Inject(LOCALE_ID) private defaultLocale: string) {
    const savedLocale = this.getSavedLocale();
    const browserLocale = this.getBrowserLocale();
    const initialLocale = savedLocale || browserLocale || this.defaultLocale || 'de';
    
    this.setCurrentLocale(initialLocale);
  }

  get currentLocale$(): Observable<string> {
    return this.currentLocaleSubject.asObservable();
  }

  get currentLocale(): string {
    return this.currentLocaleSubject.value;
  }

  setLocale(locale: string): void {
    if (this.isSupportedLocale(locale)) {
      this.setCurrentLocale(locale);
      this.saveLocale(locale);
      
      // Reload the application with the new locale
      window.location.reload();
    }
  }

  getCurrentLocaleInfo(): SupportedLocale {
    return this.supportedLocales.find(l => l.code === this.currentLocale) || this.supportedLocales[0];
  }

  isCurrentLocale(locale: string): boolean {
    return this.currentLocale === locale;
  }

  private isSupportedLocale(locale: string): boolean {
    return this.supportedLocales.some(l => l.code === locale);
  }

  private setCurrentLocale(locale: string): void {
    if (this.isSupportedLocale(locale)) {
      this.currentLocaleSubject.next(locale);
    }
  }

  private getSavedLocale(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.storageKey);
    }
    return null;
  }

  private saveLocale(locale: string): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.storageKey, locale);
    }
  }

  private getBrowserLocale(): string {
    if (typeof navigator !== 'undefined') {
      const browserLang = navigator.language || (navigator as any).userLanguage;
      const langCode = browserLang.split('-')[0];
      return this.isSupportedLocale(langCode) ? langCode : 'de';
    }
    return 'de';
  }
}