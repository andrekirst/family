import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full text-center">
        <!-- 404 Animation -->
        <div class="mb-8">
          <h1 class="text-9xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-indigo-600 to-blue-600 dark:from-indigo-400 dark:to-blue-400 animate-pulse">
            404
          </h1>
        </div>
        
        <!-- Error Message -->
        <div class="mb-8">
          <h2 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-4">
            Seite nicht gefunden
          </h2>
          <p class="text-lg text-gray-600 dark:text-gray-400">
            Die gesuchte Seite existiert leider nicht. Möglicherweise wurde sie verschoben oder entfernt.
          </p>
        </div>
        
        <!-- Illustration -->
        <div class="mb-8">
          <svg class="mx-auto h-48 w-48 text-gray-400 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="0.5" 
                  d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        
        <!-- Action Buttons -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center">
          <a routerLink="/dashboard" 
             class="inline-flex items-center justify-center px-6 py-3 text-base font-medium rounded-lg text-white bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
            </svg>
            Zur Startseite
          </a>
          
          <button onclick="history.back()" 
                  class="inline-flex items-center justify-center px-6 py-3 text-base font-medium rounded-lg text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                    d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
            </svg>
            Zurück
          </button>
        </div>
        
        <!-- Help Text -->
        <div class="mt-8 text-sm text-gray-500 dark:text-gray-400">
          <p>Benötigen Sie Hilfe?</p>
          <a href="mailto:support@family.app" 
             class="text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium">
            Kontaktieren Sie den Support
          </a>
        </div>
      </div>
    </div>
  `
})
export class NotFoundComponent {}