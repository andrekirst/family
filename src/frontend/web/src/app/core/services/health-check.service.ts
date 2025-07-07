import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface HealthStatus {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  totalDuration: string;
  entries: {
    [key: string]: {
      data?: any;
      description?: string;
      duration: string;
      status: 'Healthy' | 'Degraded' | 'Unhealthy';
      tags?: string[];
    };
  };
}

export interface ServiceHealth {
  name: string;
  status: 'Healthy' | 'Degraded' | 'Unhealthy' | 'Unknown';
  responseTime?: number;
  lastChecked: Date;
  error?: string;
  details?: any;
}

@Injectable({
  providedIn: 'root'
})
export class HealthCheckService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl.replace('/graphql', '');

  getHealthStatus(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>(`${this.apiUrl}/health`).pipe(
      catchError(error => {
        console.error('Health check failed:', error);
        return of({
          status: 'Unhealthy' as const,
          totalDuration: '0ms',
          entries: {
            'api': {
              status: 'Unhealthy' as const,
              duration: '0ms',
              description: error.message || 'Failed to connect to health endpoint'
            }
          }
        });
      })
    );
  }

  getServiceHealthStatuses(): Observable<ServiceHealth[]> {
    const startTime = Date.now();
    
    return this.getHealthStatus().pipe(
      map(health => {
        const responseTime = Date.now() - startTime;
        const services: ServiceHealth[] = [];
        
        // Add overall API health
        services.push({
          name: 'API Server',
          status: health.status,
          responseTime,
          lastChecked: new Date(),
          details: {
            totalDuration: health.totalDuration,
            endpoint: `${this.apiUrl}/health`
          }
        });
        
        // Add individual service checks
        for (const [serviceName, serviceData] of Object.entries(health.entries)) {
          services.push({
            name: this.formatServiceName(serviceName),
            status: serviceData.status,
            responseTime: this.parseDuration(serviceData.duration),
            lastChecked: new Date(),
            details: serviceData.data,
            error: serviceData.status === 'Unhealthy' ? serviceData.description : undefined
          });
        }
        
        return services;
      }),
      catchError(error => {
        return of([{
          name: 'API Server',
          status: 'Unknown' as const,
          lastChecked: new Date(),
          error: 'Unable to reach health endpoint'
        }]);
      })
    );
  }

  // Poll health status every 30 seconds
  pollHealthStatus(intervalMs: number = 30000): Observable<ServiceHealth[]> {
    return interval(intervalMs).pipe(
      switchMap(() => this.getServiceHealthStatuses())
    );
  }

  private formatServiceName(name: string): string {
    // Convert snake_case or kebab-case to Title Case
    return name
      .split(/[-_]/)
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }

  private parseDuration(duration: string): number {
    // Parse duration string like "123ms" to number
    const match = duration.match(/(\d+)ms/);
    return match ? parseInt(match[1], 10) : 0;
  }
}