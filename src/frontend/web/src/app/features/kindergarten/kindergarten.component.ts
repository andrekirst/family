import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kindergarten',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-7xl">
      <!-- Kindergarten Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-pink-600 to-rose-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- Kindergarten Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 5.9c1.16 0 2.1.94 2.1 2.1s-.94 2.1-2.1 2.1S9.9 9.16 9.9 8s.94-2.1 2.1-2.1m0 9c2.97 0 6.1 1.46 6.1 2.1v1.1H5.9V17c0-.64 3.13-2.1 6.1-2.1M12 4C9.79 4 8 5.79 8 8s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm0 9c-2.67 0-8 1.34-8 4v3h16v-3c0-2.66-5.33-4-8-4z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">Kindergartenmanagement</h1>
              <p class="text-pink-100 text-sm">Essensversorgung und Termine</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              Hier verwalten Sie alle Kindergartenangelegenheiten.
            </p>
            <div class="bg-pink-50 border-l-4 border-pink-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-pink-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
                <p class="text-pink-700 text-sm">
                  Diese Funktion wird in einer zukünftigen Version implementiert.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future Kindergarten Features -->
          <div class="mt-8 space-y-8">
            <!-- Kindergarten Overview -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div class="bg-pink-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Anmeldungen</span>
                  <svg class="w-5 h-5 text-pink-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"/>
                  </svg>
                </div>
                <div class="h-8 bg-pink-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-orange-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Essen bestellt</span>
                  <svg class="w-5 h-5 text-orange-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M8.1 13.34l2.83-2.83L12.93 12l-2.83 2.83L8.1 13.34zm6.78-1.81c1.53.71 3.68.21 5.27-1.38 1.91-1.91 2.28-4.65.81-6.12-1.46-1.46-4.2-1.1-6.12.81-1.59 1.59-2.09 3.74-1.38 5.27L3.7 19.87l1.41 1.41L12.88 11.53z"/>
                  </svg>
                </div>
                <div class="h-8 bg-orange-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-green-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Anwesenheit heute</span>
                  <svg class="w-5 h-5 text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                  </svg>
                </div>
                <div class="h-8 bg-green-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-blue-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Nächster Termin</span>
                  <svg class="w-5 h-5 text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                  </svg>
                </div>
                <div class="h-8 bg-blue-200 rounded animate-pulse"></div>
              </div>
            </div>
            
            <!-- Weekly Menu -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Wochenmenü</h2>
              <div class="bg-gray-50 rounded-lg p-4">
                <div class="grid grid-cols-1 md:grid-cols-5 gap-4">
                  <!-- Monday -->
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <h3 class="text-sm font-semibold text-gray-900 mb-2">Montag</h3>
                    <div class="space-y-2">
                      <div class="h-4 bg-pink-200 rounded animate-pulse"></div>
                      <div class="h-3 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                    <div class="mt-3">
                      <button class="text-xs text-pink-600 hover:text-pink-800">Bestellen</button>
                    </div>
                  </div>
                  
                  <!-- Tuesday -->
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <h3 class="text-sm font-semibold text-gray-900 mb-2">Dienstag</h3>
                    <div class="space-y-2">
                      <div class="h-4 bg-orange-200 rounded animate-pulse"></div>
                      <div class="h-3 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                    <div class="mt-3">
                      <span class="text-xs text-green-600 font-medium">Bestellt ✓</span>
                    </div>
                  </div>
                  
                  <!-- Wednesday -->
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <h3 class="text-sm font-semibold text-gray-900 mb-2">Mittwoch</h3>
                    <div class="space-y-2">
                      <div class="h-4 bg-green-200 rounded animate-pulse"></div>
                      <div class="h-3 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                    <div class="mt-3">
                      <button class="text-xs text-pink-600 hover:text-pink-800">Bestellen</button>
                    </div>
                  </div>
                  
                  <!-- Thursday -->
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <h3 class="text-sm font-semibold text-gray-900 mb-2">Donnerstag</h3>
                    <div class="space-y-2">
                      <div class="h-4 bg-blue-200 rounded animate-pulse"></div>
                      <div class="h-3 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                    <div class="mt-3">
                      <button class="text-xs text-pink-600 hover:text-pink-800">Bestellen</button>
                    </div>
                  </div>
                  
                  <!-- Friday -->
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <h3 class="text-sm font-semibold text-gray-900 mb-2">Freitag</h3>
                    <div class="space-y-2">
                      <div class="h-4 bg-purple-200 rounded animate-pulse"></div>
                      <div class="h-3 bg-gray-200 rounded animate-pulse"></div>
                    </div>
                    <div class="mt-3">
                      <button class="text-xs text-pink-600 hover:text-pink-800">Bestellen</button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Activities and Events -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Aktivitäten & Termine</h2>
              <div class="space-y-4">
                <!-- Activity Item 1 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-pink-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-pink-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-48 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-32 animate-pulse"></div>
                        </div>
                        <span class="text-sm text-gray-500">Morgen</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Activity Item 2 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-orange-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-orange-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-56 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-40 animate-pulse"></div>
                        </div>
                        <span class="text-sm text-gray-500">Nächste Woche</span>
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
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-pink-400 hover:bg-pink-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-pink-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M8.1 13.34l2.83-2.83L12.93 12l-2.83 2.83L8.1 13.34zm6.78-1.81c1.53.71 3.68.21 5.27-1.38 1.91-1.91 2.28-4.65.81-6.12-1.46-1.46-4.2-1.1-6.12.81-1.59 1.59-2.09 3.74-1.38 5.27L3.7 19.87l1.41 1.41L12.88 11.53z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-pink-700">Essen bestellen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-orange-400 hover:bg-orange-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-orange-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-orange-700">Termin buchen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-green-400 hover:bg-green-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-green-700">Anwesenheit melden</span>
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
export class KindergartenComponent {}