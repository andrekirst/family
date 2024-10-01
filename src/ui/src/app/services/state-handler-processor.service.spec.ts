import { TestBed } from '@angular/core/testing';

import { StateHandlerProcessorService } from './state-handler-processor.service';

describe('StateHandlerProcessorService', () => {
  let service: StateHandlerProcessorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StateHandlerProcessorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
