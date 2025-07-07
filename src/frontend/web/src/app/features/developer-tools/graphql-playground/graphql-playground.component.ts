import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { environment } from '../../../../environments/environment';
import { AccessibilityService } from '../../../core/services/accessibility.service';

@Component({
  selector: 'app-graphql-playground',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <!-- Header -->
      <div class="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex items-center justify-between">
            <div>
              <h1 class="text-2xl font-bold text-gray-900 dark:text-gray-100">GraphQL Playground</h1>
              <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
                Interaktive GraphQL-IDE für API-Exploration und -Tests
              </p>
            </div>
            <button
              (click)="openInNewTab()"
              class="inline-flex items-center px-4 py-2 text-sm font-medium rounded-lg text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
              [attr.aria-label]="'GraphQL Playground in neuem Tab öffnen'">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"/>
              </svg>
              In neuem Tab öffnen
            </button>
          </div>
        </div>
      </div>

      <!-- Main Content -->
      <div class="flex-1 p-4">
        <div class="max-w-7xl mx-auto">
          <!-- Info Box -->
          <div class="mb-4 p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/>
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-blue-800 dark:text-blue-200">
                  GraphQL Playground Features
                </h3>
                <div class="mt-2 text-sm text-blue-700 dark:text-blue-300">
                  <ul class="list-disc list-inside space-y-1">
                    <li>Interaktive Query-Erstellung mit Autocomplete</li>
                    <li>Schema-Explorer und Dokumentation</li>
                    <li>Query-History und -Favoriten</li>
                    <li>Variablen und HTTP-Header-Verwaltung</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>

          <!-- iFrame Container -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden" 
               [style.height.px]="iframeHeight">
            <iframe
              *ngIf="graphqlPlaygroundUrl"
              [src]="graphqlPlaygroundUrl"
              class="w-full h-full border-0"
              title="GraphQL Playground"
              (load)="onIframeLoad()"
              [attr.aria-label]="'Eingebettetes GraphQL Playground'">
            </iframe>
            
            <!-- Loading State -->
            <div *ngIf="!graphqlPlaygroundUrl" 
                 class="flex items-center justify-center h-96">
              <div class="text-center">
                <div class="w-16 h-16 border-4 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                <p class="text-gray-600 dark:text-gray-400">GraphQL Playground wird geladen...</p>
              </div>
            </div>
          </div>

          <!-- Error State -->
          <div *ngIf="error" 
               class="mt-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                  Fehler beim Laden
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
export class GraphqlPlaygroundComponent implements OnInit {
  graphqlPlaygroundUrl: SafeResourceUrl | null = null;
  iframeHeight = 600;
  error: string | null = null;
  
  private sanitizer = inject(DomSanitizer);
  
  ngOnInit(): void {
    this.loadGraphqlPlayground();
    this.adjustIframeHeight();
    window.addEventListener('resize', () => this.adjustIframeHeight());
  }
  
  ngOnDestroy(): void {
    window.removeEventListener('resize', () => this.adjustIframeHeight());
  }
  
  private loadGraphqlPlayground(): void {
    try {
      const graphqlEndpoint = environment.graphqlEndpoint || '/graphql';
      const playgroundUrl = `${graphqlEndpoint}/playground`;
      
      this.graphqlPlaygroundUrl = this.sanitizer.bypassSecurityTrustResourceUrl(playgroundUrl);
    } catch (error) {
      console.error('Error loading GraphQL Playground:', error);
      this.error = 'GraphQL Playground konnte nicht geladen werden. Bitte überprüfen Sie die Konfiguration.';
    }
  }
  
  private adjustIframeHeight(): void {
    const headerHeight = 180; // Approximate height of header and info box
    const padding = 32; // Padding around iframe
    this.iframeHeight = window.innerHeight - headerHeight - padding;
  }
  
  onIframeLoad(): void {
    // Handle iframe load event if needed
    console.log('GraphQL Playground loaded successfully');
  }
  
  openInNewTab(): void {
    const graphqlEndpoint = environment.graphqlEndpoint || '/graphql';
    const playgroundUrl = `${graphqlEndpoint}/playground`;
    window.open(playgroundUrl, '_blank');
  }
}