import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { Location } from '@angular/common';

export abstract class StateHandlerProcessorService {
  public abstract createState(url: string): Observable<string | undefined>;
  public abstract restoreState(state?: string): void;
}

@Injectable({
  providedIn: 'root'
})
export class StateHandlerProcessorServiceImpl implements StateHandlerProcessorService {
  
  constructor(private location: Location)
  {
  }
  
  public createState(url: string): Observable<string | undefined> {
    const externalUrl = this.location.prepareExternalUrl(url);
    const urlId = uuidv4();

    sessionStorage.setItem(urlId, externalUrl);

    return of(urlId);
  }
  
  public restoreState(state?: string): void {
    if(state == undefined) {
      return;
    }

    const url = sessionStorage.getItem(state);
    if(url == null) {
      return;
    }

    window.location.href = window.location.origin + url;
  }
}
