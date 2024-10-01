import { Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';

const STORAGE_PREFIX = 'zitadel';

@Injectable({
  providedIn: 'root'
})
export class StorageService implements OAuthStorage {
  private storage: Storage = window.localStorage;

  constructor() {}
  
  public getItem<TResult = string>(key: string): TResult | null {
    const result = this.storage.getItem(this.getPrefixedKey(key));

    return result ? JSON.parse(result) : null;
  }
  
  public removeItem(key: string): void {
    this.storage.removeItem(this.getPrefixedKey(key));
  }
  
  public setItem<TValue = string>(key: string, value: TValue): void {
    this.storage.setItem(this.getPrefixedKey(key), JSON.stringify(value));
  }

  public getPrefixedKey(key: string): string {
    return `${STORAGE_PREFIX}:${key}`;
  }
}
