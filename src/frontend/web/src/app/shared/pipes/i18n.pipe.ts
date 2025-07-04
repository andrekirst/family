import { Pipe, PipeTransform, OnDestroy } from '@angular/core';
import { I18nService } from '../../core/services/i18n.service';
import { Subscription } from 'rxjs';

@Pipe({
  name: 'i18n',
  standalone: true,
  pure: false // Make impure to respond to language changes
})
export class I18nPipe implements PipeTransform, OnDestroy {
  private subscription?: Subscription;
  private lastTranslations: any = {};

  constructor(private i18nService: I18nService) {
    // Subscribe to translation changes
    this.subscription = this.i18nService.translations$.subscribe(translations => {
      this.lastTranslations = translations;
    });
  }

  transform(key: string, params?: { [key: string]: string | number }): string {
    return this.i18nService.translate(key, params);
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}