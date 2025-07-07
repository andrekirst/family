import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { HealthCheckService, ServiceHealth } from '../../../core/services/health-check.service';

@Component({
  selector: 'app-health-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <!-- Header -->
      <div class="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex items-center justify-between">
            <div>
              <h1 class="text-2xl font-bold text-gray-900 dark:text-gray-100">Health Check Dashboard</h1>
              <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
                System-Status und Service-Überwachung
              </p>
            </div>
            <div class="flex items-center space-x-4">
              <button
                (click)="refreshHealth()"
                [disabled]="isLoading"
                class="inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
                [attr.aria-label]="'Gesundheitsstatus aktualisieren'">
                <svg class="w-5 h-5 mr-2" [class.animate-spin]="isLoading" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                        d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                </svg>
                {{ isLoading ? 'Lädt...' : 'Aktualisieren' }}
              </button>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Auto-Refresh: {{ autoRefresh ? 'An' : 'Aus' }}
              </span>
              <button
                (click)="toggleAutoRefresh()"
                class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                [class.bg-primary-600]="autoRefresh"
                [class.bg-gray-200]="!autoRefresh"
                [attr.aria-pressed]="autoRefresh"
                [attr.aria-label]="autoRefresh ? 'Auto-Refresh deaktivieren' : 'Auto-Refresh aktivieren'">
                <span
                  class="inline-block h-4 w-4 transform rounded-full bg-white transition-transform"
                  [class.translate-x-6]="autoRefresh"
                  [class.translate-x-1]="!autoRefresh">
                </span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Content -->
      <div class="p-4 sm:p-6 lg:p-8">
        <div class="max-w-7xl mx-auto">
          <!-- Overall Status -->
          <div class="mb-6 bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
            <h2 class="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">System-Gesamtstatus</h2>
            <div class="flex items-center space-x-4">
              <div class="flex-shrink-0">
                <div class="w-16 h-16 rounded-full flex items-center justify-center"
                     [ngClass]="{
                       'bg-green-100 dark:bg-green-900/30': overallStatus === 'Healthy',
                       'bg-yellow-100 dark:bg-yellow-900/30': overallStatus === 'Degraded',
                       'bg-red-100 dark:bg-red-900/30': overallStatus === 'Unhealthy',
                       'bg-gray-100 dark:bg-gray-700': overallStatus === 'Unknown'
                     }">
                  <svg class="w-8 h-8" fill="currentColor" viewBox="0 0 20 20"
                       [ngClass]="{
                         'text-green-600 dark:text-green-400': overallStatus === 'Healthy',
                         'text-yellow-600 dark:text-yellow-400': overallStatus === 'Degraded',
                         'text-red-600 dark:text-red-400': overallStatus === 'Unhealthy',
                         'text-gray-600 dark:text-gray-400': overallStatus === 'Unknown'
                       }">
                    <path *ngIf="overallStatus === 'Healthy'" fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
                    <path *ngIf="overallStatus === 'Degraded'" fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
                    <path *ngIf="overallStatus === 'Unhealthy'" fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
                    <path *ngIf="overallStatus === 'Unknown'" d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/>
                  </svg>
                </div>
              </div>
              <div class="flex-1">
                <p class="text-2xl font-bold"
                   [ngClass]="{
                     'text-green-600 dark:text-green-400': overallStatus === 'Healthy',
                     'text-yellow-600 dark:text-yellow-400': overallStatus === 'Degraded',
                     'text-red-600 dark:text-red-400': overallStatus === 'Unhealthy',
                     'text-gray-600 dark:text-gray-400': overallStatus === 'Unknown'
                   }">
                  {{ getStatusText(overallStatus) }}
                </p>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  {{ healthyServices }} von {{ totalServices }} Services sind verfügbar
                </p>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  Letzte Aktualisierung: {{ lastUpdateTime | date:'HH:mm:ss' }}
                </p>
              </div>
            </div>
          </div>

          <!-- Services Grid -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div *ngFor="let service of services" 
                 class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 hover:shadow-md transition-shadow">
              <div class="flex items-start justify-between mb-4">
                <h3 class="text-lg font-medium text-gray-900 dark:text-gray-100">{{ service.name }}</h3>
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      [ngClass]="{
                        'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400': service.status === 'Healthy',
                        'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400': service.status === 'Degraded',
                        'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400': service.status === 'Unhealthy',
                        'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300': service.status === 'Unknown'
                      }">
                  {{ getStatusText(service.status) }}
                </span>
              </div>
              
              <div class="space-y-2">
                <div *ngIf="service.responseTime !== undefined" class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">Antwortzeit:</span>
                  <span class="font-medium text-gray-900 dark:text-gray-100">{{ service.responseTime }}ms</span>
                </div>
                
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">Letzte Prüfung:</span>
                  <span class="font-medium text-gray-900 dark:text-gray-100">{{ service.lastChecked | date:'HH:mm:ss' }}</span>
                </div>
                
                <div *ngIf="service.error" class="mt-3 p-2 bg-red-50 dark:bg-red-900/20 rounded text-sm text-red-700 dark:text-red-300">
                  {{ service.error }}
                </div>
                
                <div *ngIf="service.details && !service.error" class="mt-3">
                  <button
                    (click)="toggleDetails(service)"
                    class="text-sm text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 focus:outline-none focus:underline">
                    {{ isDetailsExpanded(service) ? 'Details ausblenden' : 'Details anzeigen' }}
                  </button>
                  <div *ngIf="isDetailsExpanded(service)" class="mt-2 p-2 bg-gray-50 dark:bg-gray-700 rounded text-xs font-mono overflow-x-auto">
                    <pre>{{ service.details | json }}</pre>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Error State -->
          <div *ngIf="error" 
               class="mt-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                  Fehler beim Abrufen der Gesundheitsdaten
                </h3>
                <div class="mt-2 text-sm text-red-700 dark:text-red-300">
                  {{ error }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class HealthDashboardComponent implements OnInit, OnDestroy {
  services: ServiceHealth[] = [];
  isLoading = false;
  error: string | null = null;
  autoRefresh = true;
  lastUpdateTime = new Date();
  expandedServices = new Set<string>();
  
  private destroy$ = new Subject<void>();
  private healthCheckService = inject(HealthCheckService);
  
  get overallStatus(): 'Healthy' | 'Degraded' | 'Unhealthy' | 'Unknown' {
    if (this.services.length === 0) return 'Unknown';
    
    const hasUnhealthy = this.services.some(s => s.status === 'Unhealthy');
    const hasDegraded = this.services.some(s => s.status === 'Degraded');
    const hasUnknown = this.services.some(s => s.status === 'Unknown');
    
    if (hasUnhealthy) return 'Unhealthy';
    if (hasDegraded) return 'Degraded';
    if (hasUnknown) return 'Unknown';
    return 'Healthy';
  }
  
  get healthyServices(): number {
    return this.services.filter(s => s.status === 'Healthy').length;
  }
  
  get totalServices(): number {
    return this.services.length;
  }
  
  ngOnInit(): void {
    this.loadHealthStatus();
    this.startAutoRefresh();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  refreshHealth(): void {
    this.loadHealthStatus();
  }
  
  toggleAutoRefresh(): void {
    this.autoRefresh = !this.autoRefresh;
    if (this.autoRefresh) {
      this.startAutoRefresh();
    } else {
      this.destroy$.next();
    }
  }
  
  toggleDetails(service: ServiceHealth): void {
    if (this.expandedServices.has(service.name)) {
      this.expandedServices.delete(service.name);
    } else {
      this.expandedServices.add(service.name);
    }
  }
  
  isDetailsExpanded(service: ServiceHealth): boolean {
    return this.expandedServices.has(service.name);
  }
  
  getStatusText(status: string): string {
    switch (status) {
      case 'Healthy': return 'Gesund';
      case 'Degraded': return 'Beeinträchtigt';
      case 'Unhealthy': return 'Ungesund';
      case 'Unknown': return 'Unbekannt';
      default: return status;
    }
  }
  
  private loadHealthStatus(): void {
    this.isLoading = true;
    this.error = null;
    
    this.healthCheckService.getServiceHealthStatuses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (services) => {
          this.services = services;
          this.lastUpdateTime = new Date();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading health status:', error);
          this.error = 'Fehler beim Laden der Gesundheitsdaten. Bitte versuchen Sie es später erneut.';
          this.isLoading = false;
        }
      });
  }
  
  private startAutoRefresh(): void {
    if (!this.autoRefresh) return;
    
    this.healthCheckService.pollHealthStatus(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (services) => {
          if (this.autoRefresh) {
            this.services = services;
            this.lastUpdateTime = new Date();
          }
        },
        error: (error) => {
          console.error('Error in auto-refresh:', error);
        }
      });
  }
}