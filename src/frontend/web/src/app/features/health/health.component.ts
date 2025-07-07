import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-health',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-7xl">
      <!-- Health Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-green-600 to-teal-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- Health Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M19 8h-2v3h-3v2h3v3h2v-3h3v-2h-3zM2 12v2h8v-2zm11-2h2V7h3V5h-3V2h-2v3h-3v2h3zm-2 8h2v-3h2v-2h-4z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">Gesundheitsverwaltung</h1>
              <p class="text-green-100 text-sm">Arztbesuche, Medikamente und Impfungen</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              Verwalten Sie alle gesundheitsbezogenen Informationen Ihrer Familie.
            </p>
            <div class="bg-emerald-50 border-l-4 border-emerald-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-emerald-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                </svg>
                <p class="text-emerald-700 text-sm">
                  Diese Funktion wird in einer zukünftigen Version implementiert.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future Health Sections -->
          <div class="mt-8 space-y-8">
            <!-- Health Overview -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div class="bg-green-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Nächster Arzttermin</span>
                  <svg class="w-5 h-5 text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                  </svg>
                </div>
                <div class="h-8 bg-green-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-blue-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Aktive Medikamente</span>
                  <svg class="w-5 h-5 text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M4.22 11.29l1.41-1.41 5.66 5.66-1.41 1.41L4.22 11.29zM11 7l2 2-7 7-2-2 7-7zm6-2.71l-4.3 4.29 1.42 1.41L18.41 5.7 19.82 7.11 21.24 5.7 19.82 4.29 18.41 2.88 17 4.29l1.41 1.41zM3.52 20.48l2.83-2.83 1.41 1.41-2.83 2.83c-.78.78-2.05.78-2.83 0-.78-.78-.78-2.05 0-2.83.78-.78 2.05-.78 2.83 0z"/>
                  </svg>
                </div>
                <div class="h-8 bg-blue-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-yellow-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Fällige Impfungen</span>
                  <svg class="w-5 h-5 text-yellow-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                  </svg>
                </div>
                <div class="h-8 bg-yellow-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-purple-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Notfallkontakte</span>
                  <svg class="w-5 h-5 text-purple-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
                  </svg>
                </div>
                <div class="h-8 bg-purple-200 rounded animate-pulse"></div>
              </div>
            </div>
            
            <!-- Medical Records -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Medizinische Aufzeichnungen</h2>
              <div class="space-y-4">
                <!-- Record Item 1 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-48 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-32 animate-pulse"></div>
                        </div>
                        <span class="text-sm text-gray-500">vor 2 Tagen</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Record Item 2 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 3c1.93 0 3.5 1.57 3.5 3.5S13.93 13 12 13s-3.5-1.57-3.5-3.5S10.07 6 12 6zm7 13H5v-.23c0-.62.28-1.2.76-1.58C7.47 15.82 9.64 15 12 15s4.53.82 6.24 2.19c.48.38.76.97.76 1.58V19z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-56 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-40 animate-pulse"></div>
                        </div>
                        <span class="text-sm text-gray-500">vor 1 Woche</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Quick Actions -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Schnellaktionen</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-green-400 hover:bg-green-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-green-700">Arzttermin buchen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-blue-400 hover:bg-blue-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-blue-700">Medikament hinzufügen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-yellow-400 hover:bg-yellow-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-yellow-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12 2l-5.5 9h11z M12 5.84L13.93 9h-3.87z M17.5 13c-2.49 0-4.5 2.01-4.5 4.5s2.01 4.5 4.5 4.5 4.5-2.01 4.5-4.5-2.01-4.5-4.5-4.5zm-2 5h4v1h-4z M3 9.5c0 1.38 1.12 2.5 2.5 2.5S8 10.88 8 9.5 6.88 7 5.5 7 3 8.12 3 9.5z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-yellow-700">Impfung eintragen</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class HealthComponent {}