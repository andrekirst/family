import { Injectable, Injector, OnDestroy, Type } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { filter, map, Observable, shareReplay, Subject, switchMap, take, takeUntil, throwError } from 'rxjs';
import { StateHandlerProcessorService } from './state-handler-processor.service';
import { GuardsCheckStart, Router, RouterEvent } from '@angular/router';

export abstract class StateHandlerService {
  public abstract createState(): Observable<string | undefined>;
  public abstract initStateHandler(): void;
}

@Injectable({
  providedIn: 'root'
})
export class StateHandlerServiceImpl implements StateHandlerService, OnDestroy {
  private events?: Observable<string>;
  private unsubscribe$: Subject<void> = new Subject();  

  constructor(
    oAuthService: OAuthService,
    private injector: Injector,
    private processor: StateHandlerProcessorService) {
      oAuthService.events
      .pipe(
        filter(event => event.type === 'token_received'),
        map(() => oAuthService.state),
        takeUntil(this.unsubscribe$)
      )
      .subscribe(state => processor.restoreState(state));
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
  }
  
  public createState(): Observable<string | undefined> {
    if(this.events == undefined) {
      return throwError(() => new Error('no router events'));
    }

    return this.events.pipe(
      take(1),
      switchMap(url => this.processor.createState(url))
    );
  }
  
  public initStateHandler(): void {
    const router = this.injector.get(Router as Type<Router>);
    this.events = (router.events as Observable<RouterEvent>)
      .pipe(
        filter(event => event instanceof GuardsCheckStart),
        map(event => event.url),
        shareReplay(1)
      );

      this.events
        .pipe(takeUntil(this.unsubscribe$))
        .subscribe();
  }
}
