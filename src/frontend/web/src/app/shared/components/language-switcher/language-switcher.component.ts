import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { LocalizationService, SupportedLocale } from '../../../core/services/localization.service';
import { Observable } from 'rxjs';
import { I18nPipe } from '../../pipes/i18n.pipe';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatMenuModule,
    MatIconModule,
    I18nPipe
  ],
  template: `
    <button 
      mat-button 
      [matMenuTriggerFor]="languageMenu"
      class="language-switcher-button"
      [attr.aria-label]="'language.switch' | i18n">
      <mat-icon>language</mat-icon>
      <span class="language-flag">{{ currentLocaleInfo.flag }}</span>
      <span class="language-name">{{ currentLocaleInfo.name }}</span>
      <mat-icon>arrow_drop_down</mat-icon>
    </button>

    <mat-menu #languageMenu="matMenu" class="language-menu">
      <button 
        mat-menu-item 
        *ngFor="let locale of supportedLocales" 
        [class.active]="localizationService.isCurrentLocale(locale.code)"
        (click)="switchLanguage(locale.code)"
        [attr.aria-label]="'language.switch' | i18n">
        <span class="language-flag">{{ locale.flag }}</span>
        <span class="language-name">{{ locale.name }}</span>
        <mat-icon *ngIf="localizationService.isCurrentLocale(locale.code)">check</mat-icon>
      </button>
    </mat-menu>
  `,
  styles: [`
    .language-switcher-button {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      min-width: auto;
    }

    .language-flag {
      font-size: 18px;
      line-height: 1;
    }

    .language-name {
      font-size: 14px;
      font-weight: 500;
    }

    .language-menu {
      min-width: 200px;
    }

    .language-menu button {
      display: flex;
      align-items: center;
      gap: 12px;
      width: 100%;
      padding: 12px 16px;
    }

    .language-menu button.active {
      background-color: rgba(0, 0, 0, 0.04);
      font-weight: 500;
    }

    .language-menu .language-flag {
      font-size: 20px;
      line-height: 1;
      width: 24px;
      text-align: center;
    }

    .language-menu .language-name {
      flex: 1;
      text-align: left;
    }

    .language-menu mat-icon {
      color: #4CAF50;
    }

    @media (max-width: 768px) {
      .language-switcher-button .language-name {
        display: none;
      }
    }
  `]
})
export class LanguageSwitcherComponent implements OnInit {
  supportedLocales: SupportedLocale[] = [];
  currentLocaleInfo: SupportedLocale = { code: 'de', name: 'Deutsch', flag: '🇩🇪' };
  currentLocale$: Observable<string>;
  
  public localizationService = inject(LocalizationService);

  constructor() {
    this.currentLocale$ = this.localizationService.currentLocale$;
  }

  ngOnInit(): void {
    this.supportedLocales = this.localizationService.supportedLocales;
    this.updateCurrentLocaleInfo();
    
    // Subscribe to locale changes
    this.currentLocale$.subscribe(() => {
      this.updateCurrentLocaleInfo();
    });
  }

  switchLanguage(locale: string): void {
    this.localizationService.setLocale(locale);
  }

  private updateCurrentLocaleInfo(): void {
    this.currentLocaleInfo = this.localizationService.getCurrentLocaleInfo();
  }
}