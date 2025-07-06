import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-7xl">
      <!-- Calendar Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-blue-600 to-indigo-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- Calendar Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">Familienkalender</h1>
              <p class="text-blue-100 text-sm">Termine und Ereignisse verwalten</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              Hier werden alle Familientermine angezeigt und verwaltet.
            </p>
            <div class="bg-indigo-50 border-l-4 border-indigo-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-indigo-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M13 3c-4.97 0-9 4.03-9 9H1l3.89 3.89.07.14L9 12H6c0-3.87 3.13-7 7-7s7 3.13 7 7-3.13 7-7 7c-1.93 0-3.68-.79-4.94-2.06l-1.42 1.42C8.27 19.99 10.51 21 13 21c4.97 0 9-4.03 9-9s-4.03-9-9-9zm-1 5v5l4.28 2.54.72-1.21-3.5-2.08V8H12z"/>
                </svg>
                <p class="text-indigo-700 text-sm">
                  Diese Funktion wird in einer zukünftigen Version implementiert.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future Calendar Preview -->
          <div class="mt-8">
            <!-- Month Header -->
            <div class="flex items-center justify-between mb-6">
              <h2 class="text-xl font-semibold text-gray-900">November 2024</h2>
              <div class="flex space-x-2">
                <button class="p-2 rounded-lg hover:bg-gray-100 transition-colors" aria-label="Vorheriger Monat">
                  <svg class="w-5 h-5 text-gray-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z"/>
                  </svg>
                </button>
                <button class="p-2 rounded-lg hover:bg-gray-100 transition-colors" aria-label="Nächster Monat">
                  <svg class="w-5 h-5 text-gray-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/>
                  </svg>
                </button>
              </div>
            </div>
            
            <!-- Calendar Grid -->
            <div class="border rounded-lg overflow-hidden">
              <!-- Weekday Headers -->
              <div class="grid grid-cols-7 bg-gray-50 border-b">
                <div class="p-3 text-center text-sm font-medium text-gray-700">Mo</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">Di</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">Mi</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">Do</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">Fr</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">Sa</div>
                <div class="p-3 text-center text-sm font-medium text-gray-700">So</div>
              </div>
              
              <!-- Calendar Days -->
              <div class="grid grid-cols-7">
                <!-- Week 1 -->
                <div class="h-20 border-r border-b bg-gray-50"></div>
                <div class="h-20 border-r border-b bg-gray-50"></div>
                <div class="h-20 border-r border-b bg-gray-50"></div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700">1</span>
                </div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700">2</span>
                </div>
                <div class="h-20 border-r border-b p-2 bg-blue-50">
                  <span class="text-sm text-gray-700">3</span>
                </div>
                <div class="h-20 border-b p-2 bg-blue-50">
                  <span class="text-sm text-gray-700">4</span>
                </div>
                
                <!-- Week 2 (showing loading state) -->
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700 mb-1 block">5</span>
                  <div class="h-2 bg-gray-200 rounded animate-pulse"></div>
                </div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700">6</span>
                </div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700 mb-1 block">7</span>
                  <div class="h-2 bg-blue-200 rounded animate-pulse"></div>
                </div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700">8</span>
                </div>
                <div class="h-20 border-r border-b p-2">
                  <span class="text-sm text-gray-700">9</span>
                </div>
                <div class="h-20 border-r border-b p-2 bg-blue-50">
                  <span class="text-sm text-gray-700">10</span>
                </div>
                <div class="h-20 border-b p-2 bg-blue-50">
                  <span class="text-sm text-gray-700">11</span>
                </div>
              </div>
            </div>
            
            <!-- Event List -->
            <div class="mt-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Anstehende Termine</h3>
              <div class="space-y-3">
                <div class="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                  <div class="flex-shrink-0">
                    <div class="w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
                  </div>
                  <div class="flex-1">
                    <div class="h-4 bg-gray-200 rounded w-3/4 mb-2 animate-pulse"></div>
                    <div class="h-3 bg-gray-200 rounded w-1/2 animate-pulse"></div>
                  </div>
                </div>
                <div class="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                  <div class="flex-shrink-0">
                    <div class="w-2 h-2 bg-green-500 rounded-full mt-2"></div>
                  </div>
                  <div class="flex-1">
                    <div class="h-4 bg-gray-200 rounded w-2/3 mb-2 animate-pulse"></div>
                    <div class="h-3 bg-gray-200 rounded w-1/3 animate-pulse"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class CalendarComponent {}