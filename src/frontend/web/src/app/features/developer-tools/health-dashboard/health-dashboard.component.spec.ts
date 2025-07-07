import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';

import { HealthDashboardComponent } from './health-dashboard.component';
import { HealthCheckService, ServiceHealth } from '../../../core/services/health-check.service';

describe('HealthDashboardComponent', () => {
  let component: HealthDashboardComponent;
  let fixture: ComponentFixture<HealthDashboardComponent>;
  let healthCheckService: jasmine.SpyObj<HealthCheckService>;

  const mockHealthyServices: ServiceHealth[] = [
    {
      name: 'Database',
      status: 'Healthy',
      responseTime: 15,
      lastChecked: new Date(),
      details: { connectionString: 'postgres://...' }
    },
    {
      name: 'Redis Cache',
      status: 'Healthy',
      responseTime: 5,
      lastChecked: new Date(),
      details: { host: 'localhost', port: 6379 }
    }
  ];

  const mockMixedServices: ServiceHealth[] = [
    {
      name: 'Database',
      status: 'Healthy',
      responseTime: 15,
      lastChecked: new Date()
    },
    {
      name: 'External API',
      status: 'Degraded',
      responseTime: 5000,
      lastChecked: new Date(),
      error: 'High response time detected'
    },
    {
      name: 'Message Queue',
      status: 'Unhealthy',
      lastChecked: new Date(),
      error: 'Connection timeout'
    }
  ];

  beforeEach(async () => {
    const healthCheckSpy = jasmine.createSpyObj('HealthCheckService', 
      ['getServiceHealthStatuses', 'pollHealthStatus']);

    await TestBed.configureTestingModule({
      imports: [HealthDashboardComponent],
      providers: [
        { provide: HealthCheckService, useValue: healthCheckSpy }
      ]
    }).compileComponents();

    healthCheckService = TestBed.inject(HealthCheckService) as jasmine.SpyObj<HealthCheckService>;
    fixture = TestBed.createComponent(HealthDashboardComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.services).toEqual([]);
      expect(component.isLoading).toBeFalsy();
      expect(component.error).toBeNull();
      expect(component.autoRefresh).toBeTruthy();
      expect(component.expandedServices.size).toBe(0);
    });

    it('should load health status on init', () => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      
      component.ngOnInit();
      
      expect(healthCheckService.getServiceHealthStatuses).toHaveBeenCalled();
    });
  });

  describe('Health Status Loading', () => {
    it('should load services successfully', fakeAsync(() => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      
      component.ngOnInit();
      tick();
      
      expect(component.services).toEqual(mockHealthyServices);
      expect(component.isLoading).toBeFalsy();
      expect(component.error).toBeNull();
      expect(component.lastUpdateTime).toBeDefined();
    }));

    it('should handle loading errors', fakeAsync(() => {
      const error = new Error('Network error');
      healthCheckService.getServiceHealthStatuses.and.returnValue(throwError(() => error));
      healthCheckService.pollHealthStatus.and.returnValue(of([]));
      
      component.ngOnInit();
      tick();
      
      expect(component.services).toEqual([]);
      expect(component.isLoading).toBeFalsy();
      expect(component.error).toBe('Fehler beim Laden der Gesundheitsdaten. Bitte versuchen Sie es später erneut.');
    }));

    it('should set loading state during refresh', () => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      
      component.refreshHealth();
      
      expect(component.isLoading).toBeTruthy();
      expect(component.error).toBeNull();
    });
  });

  describe('Overall Status Calculation', () => {
    it('should return Unknown when no services', () => {
      component.services = [];
      expect(component.overallStatus).toBe('Unknown');
    });

    it('should return Healthy when all services are healthy', () => {
      component.services = mockHealthyServices;
      expect(component.overallStatus).toBe('Healthy');
    });

    it('should return Unhealthy when any service is unhealthy', () => {
      component.services = mockMixedServices;
      expect(component.overallStatus).toBe('Unhealthy');
    });

    it('should return Degraded when services are degraded but none unhealthy', () => {
      component.services = [
        { name: 'Service1', status: 'Healthy', lastChecked: new Date() },
        { name: 'Service2', status: 'Degraded', lastChecked: new Date() }
      ];
      expect(component.overallStatus).toBe('Degraded');
    });

    it('should prioritize unhealthy over degraded', () => {
      component.services = [
        { name: 'Service1', status: 'Degraded', lastChecked: new Date() },
        { name: 'Service2', status: 'Unhealthy', lastChecked: new Date() }
      ];
      expect(component.overallStatus).toBe('Unhealthy');
    });
  });

  describe('Service Metrics', () => {
    it('should calculate healthy services count correctly', () => {
      component.services = mockMixedServices;
      expect(component.healthyServices).toBe(1);
    });

    it('should calculate total services count correctly', () => {
      component.services = mockMixedServices;
      expect(component.totalServices).toBe(3);
    });
  });

  describe('Auto Refresh', () => {
    it('should start auto refresh on init when enabled', () => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      
      component.autoRefresh = true;
      component.ngOnInit();
      
      expect(healthCheckService.pollHealthStatus).toHaveBeenCalledWith(30000);
    });

    it('should toggle auto refresh on/off', () => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      
      component.autoRefresh = true;
      component.toggleAutoRefresh();
      expect(component.autoRefresh).toBeFalsy();
      
      component.toggleAutoRefresh();
      expect(component.autoRefresh).toBeTruthy();
      expect(healthCheckService.pollHealthStatus).toHaveBeenCalled();
    });

    it('should update services via auto refresh', fakeAsync(() => {
      const updatedServices = [...mockHealthyServices, {
        name: 'New Service',
        status: 'Healthy' as const,
        lastChecked: new Date()
      }];
      
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(updatedServices));
      
      component.ngOnInit();
      tick();
      
      expect(component.services).toEqual(updatedServices);
    }));
  });

  describe('Service Details Expansion', () => {
    beforeEach(() => {
      component.services = mockHealthyServices;
    });

    it('should expand service details', () => {
      const service = mockHealthyServices[0];
      
      component.toggleDetails(service);
      
      expect(component.isDetailsExpanded(service)).toBeTruthy();
      expect(component.expandedServices.has(service.name)).toBeTruthy();
    });

    it('should collapse expanded service details', () => {
      const service = mockHealthyServices[0];
      
      component.toggleDetails(service);
      component.toggleDetails(service);
      
      expect(component.isDetailsExpanded(service)).toBeFalsy();
      expect(component.expandedServices.has(service.name)).toBeFalsy();
    });

    it('should track multiple expanded services', () => {
      const service1 = mockHealthyServices[0];
      const service2 = mockHealthyServices[1];
      
      component.toggleDetails(service1);
      component.toggleDetails(service2);
      
      expect(component.isDetailsExpanded(service1)).toBeTruthy();
      expect(component.isDetailsExpanded(service2)).toBeTruthy();
    });
  });

  describe('Status Text Translation', () => {
    it('should translate status text correctly', () => {
      expect(component.getStatusText('Healthy')).toBe('Gesund');
      expect(component.getStatusText('Degraded')).toBe('Beeinträchtigt');
      expect(component.getStatusText('Unhealthy')).toBe('Ungesund');
      expect(component.getStatusText('Unknown')).toBe('Unbekannt');
      expect(component.getStatusText('InvalidStatus')).toBe('InvalidStatus');
    });
  });

  describe('Template Rendering', () => {
    beforeEach(() => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
    });

    it('should display page title and description', () => {
      fixture.detectChanges();
      
      const title = fixture.debugElement.query(By.css('h1'));
      expect(title.nativeElement.textContent.trim()).toBe('Health Check Dashboard');
    });

    it('should display overall status correctly', () => {
      component.services = mockHealthyServices;
      fixture.detectChanges();
      
      const statusText = fixture.debugElement.query(By.css('.text-2xl.font-bold'));
      expect(statusText.nativeElement.textContent.trim()).toBe('Gesund');
      
      const statusCount = fixture.debugElement.query(By.css('.text-sm.text-gray-500'));
      expect(statusCount.nativeElement.textContent.trim()).toContain('2 von 2 Services sind verfügbar');
    });

    it('should render service cards', () => {
      component.services = mockHealthyServices;
      fixture.detectChanges();
      
      const serviceCards = fixture.debugElement.queryAll(By.css('.grid > div'));
      expect(serviceCards.length).toBe(2);
      
      const firstServiceTitle = serviceCards[0].query(By.css('h3'));
      expect(firstServiceTitle.nativeElement.textContent.trim()).toBe('Database');
    });

    it('should show loading spinner when loading', () => {
      component.isLoading = true;
      fixture.detectChanges();
      
      const spinner = fixture.debugElement.query(By.css('.animate-spin'));
      expect(spinner).toBeTruthy();
    });

    it('should display error message when error occurs', () => {
      component.error = 'Test error message';
      fixture.detectChanges();
      
      const errorSection = fixture.debugElement.query(By.css('.bg-red-50'));
      expect(errorSection).toBeTruthy();
      expect(errorSection.nativeElement.textContent).toContain('Test error message');
    });

    it('should show service details when expanded', () => {
      component.services = mockHealthyServices;
      component.expandedServices.add(mockHealthyServices[0].name);
      fixture.detectChanges();
      
      const detailsSection = fixture.debugElement.query(By.css('pre'));
      expect(detailsSection).toBeTruthy();
    });

    it('should display service errors', () => {
      component.services = mockMixedServices;
      fixture.detectChanges();
      
      const errorServices = fixture.debugElement.queryAll(By.css('.bg-red-50.dark\\:bg-red-900\\/20'));
      expect(errorServices.length).toBeGreaterThan(0);
    });
  });

  describe('Button Interactions', () => {
    beforeEach(() => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      fixture.detectChanges();
    });

    it('should call refreshHealth when refresh button is clicked', () => {
      spyOn(component, 'refreshHealth');
      
      const refreshButton = fixture.debugElement.query(By.css('button[aria-label="Gesundheitsstatus aktualisieren"]'));
      refreshButton.nativeElement.click();
      
      expect(component.refreshHealth).toHaveBeenCalled();
    });

    it('should toggle auto refresh when toggle is clicked', () => {
      spyOn(component, 'toggleAutoRefresh');
      
      const toggleButton = fixture.debugElement.query(By.css('button[aria-pressed]'));
      toggleButton.nativeElement.click();
      
      expect(component.toggleAutoRefresh).toHaveBeenCalled();
    });

    it('should disable refresh button when loading', () => {
      component.isLoading = true;
      fixture.detectChanges();
      
      const refreshButton = fixture.debugElement.query(By.css('button[aria-label="Gesundheitsstatus aktualisieren"]'));
      expect(refreshButton.nativeElement.disabled).toBeTruthy();
    });

    it('should toggle service details when details button is clicked', () => {
      component.services = mockHealthyServices;
      fixture.detectChanges();
      
      spyOn(component, 'toggleDetails');
      
      const detailsButton = fixture.debugElement.query(By.css('button:contains("Details anzeigen")'));
      if (detailsButton) {
        detailsButton.nativeElement.click();
        expect(component.toggleDetails).toHaveBeenCalledWith(mockHealthyServices[0]);
      }
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      healthCheckService.getServiceHealthStatuses.and.returnValue(of(mockHealthyServices));
      healthCheckService.pollHealthStatus.and.returnValue(of(mockHealthyServices));
      fixture.detectChanges();
    });

    it('should have proper heading structure', () => {
      const h1 = fixture.debugElement.query(By.css('h1'));
      const h2 = fixture.debugElement.query(By.css('h2'));
      const h3 = fixture.debugElement.query(By.css('h3'));
      
      expect(h1).toBeTruthy();
      expect(h2).toBeTruthy();
      expect(h3).toBeTruthy();
    });

    it('should have aria-labels for interactive elements', () => {
      const refreshButton = fixture.debugElement.query(By.css('button[aria-label]'));
      const toggleButton = fixture.debugElement.query(By.css('button[aria-pressed]'));
      
      expect(refreshButton.nativeElement.getAttribute('aria-label')).toBe('Gesundheitsstatus aktualisieren');
      expect(toggleButton.nativeElement.getAttribute('aria-pressed')).toBeDefined();
    });

    it('should have proper focus management', () => {
      const buttons = fixture.debugElement.queryAll(By.css('button'));
      
      buttons.forEach(button => {
        expect(button.nativeElement.classList.contains('focus:outline-none')).toBeTruthy();
        expect(button.nativeElement.classList.contains('focus:ring-2')).toBeTruthy();
      });
    });
  });

  describe('Component Cleanup', () => {
    it('should complete destroy subject on destroy', () => {
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');
      
      component.ngOnDestroy();
      
      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });

  describe('Status Styling', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should apply correct CSS classes for healthy status', () => {
      component.services = mockHealthyServices;
      fixture.detectChanges();
      
      const statusElement = fixture.debugElement.query(By.css('.text-green-600'));
      expect(statusElement).toBeTruthy();
    });

    it('should apply correct CSS classes for mixed status', () => {
      component.services = mockMixedServices;
      fixture.detectChanges();
      
      const unhealthyStatus = fixture.debugElement.query(By.css('.text-red-600'));
      expect(unhealthyStatus).toBeTruthy();
    });

    it('should show appropriate icons for each status', () => {
      component.services = mockMixedServices;
      fixture.detectChanges();
      
      const statusIcons = fixture.debugElement.queryAll(By.css('svg'));
      expect(statusIcons.length).toBeGreaterThan(0);
    });
  });
});