import { TestBed } from '@angular/core/testing';

import { StateHandlerService } from './state-handler.service';

describe('StateHandlerService', () => {
  let service: StateHandlerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StateHandlerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
