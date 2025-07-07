import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gradient-to-br from-red-50 to-orange-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full text-center">
        <!-- 500 Animation -->
        <div class="mb-8">
          <h1 class="text-9xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-600 dark:from-red-400 dark:to-orange-400 animate-pulse">
            500
          </h1>
        </div>
        
        <!-- Error Message -->
        <div class="mb-8">
          <h2 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-4">
            Serverfehler
          </h2>
          <p class="text-lg text-gray-600 dark:text-gray-400">
            Es ist ein unerwarteter Fehler aufgetreten. Unser Team wurde benachrichtigt und arbeitet an einer Lösung.
          </p>
        </div>
        
        <!-- Illustration -->
        <div class="mb-8">
          <svg class="mx-auto h-48 w-48 text-gray-400 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="0.5" 
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        
        <!-- Status Information -->
        <div class="mb-8 p-4 bg-white dark:bg-gray-800 rounded-lg shadow-sm">
          <div class="flex items-center justify-center space-x-2 text-sm">
            <div class="w-3 h-3 bg-red-500 rounded-full animate-pulse"></div>
            <span class="text-gray-600 dark:text-gray-400">System-Status: Fehler</span>
          </div>
        </div>
        
        <!-- Action Buttons -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center">
          <button onclick="window.location.reload()" 
                  class="inline-flex items-center justify-center px-6 py-3 text-base font-medium rounded-lg text-white bg-red-600 hover:bg-red-700 dark:bg-red-500 dark:hover:bg-red-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
            </svg>
            Seite neu laden
          </button>
          
          <a routerLink="/dashboard" 
             class="inline-flex items-center justify-center px-6 py-3 text-base font-medium rounded-lg text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
            </svg>
            Zur Startseite
          </a>
        </div>
        
        <!-- Error Details (for developers) -->
        <details class="mt-8 text-left">
          <summary class="cursor-pointer text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300">
            Technische Details anzeigen
          </summary>
          <div class="mt-4 p-4 bg-gray-100 dark:bg-gray-800 rounded-lg text-xs font-mono text-gray-700 dark:text-gray-300">
            <p>Zeitstempel: {{ timestamp }}</p>
            <p>Fehler-ID: {{ errorId }}</p>
            <p>Status: Internal Server Error (500)</p>
          </div>
        </details>
        
        <!-- Help Text -->
        <div class="mt-8 text-sm text-gray-500 dark:text-gray-400">
          <p>Falls das Problem weiterhin besteht:</p>
          <a href="mailto:support@family.app" 
             class="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 font-medium">
            Kontaktieren Sie den Support
          </a>
        </div>
      </div>
    </div>
  `
})
export class ServerErrorComponent {
  timestamp = new Date().toISOString();
  errorId = this.generateErrorId();
  
  private generateErrorId(): string {
    return Math.random().toString(36).substring(2, 9).toUpperCase();
  }
}