import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-school',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-7xl">
      <!-- School Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-orange-600 to-red-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- School Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82zM12 3L1 9l11 6 9-4.91V17h2V9L12 3z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">Schulorganisation</h1>
              <p class="text-orange-100 text-sm">Stundenpläne, Noten und Hausaufgaben</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              Verwalten Sie hier alle schulischen Angelegenheiten Ihrer Kinder.
            </p>
            <div class="bg-orange-50 border-l-4 border-orange-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-orange-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
                <p class="text-orange-700 text-sm">
                  Diese Funktion wird in einer zukünftigen Version implementiert.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future School Features -->
          <div class="mt-8 space-y-8">
            <!-- School Overview -->
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div class="bg-orange-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Aktuelle Klassen</span>
                  <svg class="w-5 h-5 text-orange-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M18 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zM6 4h5v8l-2.5-1.5L6 12V4z"/>
                  </svg>
                </div>
                <div class="h-8 bg-orange-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-red-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Offene Hausaufgaben</span>
                  <svg class="w-5 h-5 text-red-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z"/>
                  </svg>
                </div>
                <div class="h-8 bg-red-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-green-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Durchschnittsnote</span>
                  <svg class="w-5 h-5 text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                  </svg>
                </div>
                <div class="h-8 bg-green-200 rounded animate-pulse"></div>
              </div>
              <div class="bg-blue-50 rounded-lg p-4">
                <div class="flex items-center justify-between mb-2">
                  <span class="text-sm font-medium text-gray-600">Nächste Prüfung</span>
                  <svg class="w-5 h-5 text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                  </svg>
                </div>
                <div class="h-8 bg-blue-200 rounded animate-pulse"></div>
              </div>
            </div>
            
            <!-- Schedule Preview -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Stundenplan (Heute)</h2>
              <div class="bg-gray-50 rounded-lg p-4">
                <div class="space-y-3">
                  <!-- Schedule Item 1 -->
                  <div class="flex items-center justify-between p-3 bg-white rounded-lg shadow-sm">
                    <div class="flex items-center space-x-3">
                      <div class="w-2 h-12 bg-orange-500 rounded"></div>
                      <div>
                        <div class="h-4 bg-gray-200 rounded w-24 mb-1 animate-pulse"></div>
                        <div class="h-3 bg-gray-200 rounded w-16 animate-pulse"></div>
                      </div>
                    </div>
                    <div class="text-sm text-gray-500">08:00 - 08:45</div>
                  </div>
                  
                  <!-- Schedule Item 2 -->
                  <div class="flex items-center justify-between p-3 bg-white rounded-lg shadow-sm">
                    <div class="flex items-center space-x-3">
                      <div class="w-2 h-12 bg-blue-500 rounded"></div>
                      <div>
                        <div class="h-4 bg-gray-200 rounded w-20 mb-1 animate-pulse"></div>
                        <div class="h-3 bg-gray-200 rounded w-12 animate-pulse"></div>
                      </div>
                    </div>
                    <div class="text-sm text-gray-500">08:50 - 09:35</div>
                  </div>
                  
                  <!-- Schedule Item 3 -->
                  <div class="flex items-center justify-between p-3 bg-white rounded-lg shadow-sm">
                    <div class="flex items-center space-x-3">
                      <div class="w-2 h-12 bg-green-500 rounded"></div>
                      <div>
                        <div class="h-4 bg-gray-200 rounded w-28 mb-1 animate-pulse"></div>
                        <div class="h-3 bg-gray-200 rounded w-20 animate-pulse"></div>
                      </div>
                    </div>
                    <div class="text-sm text-gray-500">09:55 - 10:40</div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Homework and Tasks -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">Aufgaben & Hausaufgaben</h2>
              <div class="space-y-4">
                <!-- Homework Item 1 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-red-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-48 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-32 animate-pulse"></div>
                        </div>
                        <div class="flex items-center space-x-2">
                          <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                            Fällig morgen
                          </span>
                          <input type="checkbox" class="w-4 h-4 text-orange-600 border-gray-300 rounded focus:ring-orange-500">
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Homework Item 2 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-start space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center">
                        <svg class="w-6 h-6 text-yellow-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="flex items-center justify-between">
                        <div>
                          <div class="h-5 bg-gray-200 rounded w-56 mb-2 animate-pulse"></div>
                          <div class="h-4 bg-gray-200 rounded w-40 animate-pulse"></div>
                        </div>
                        <div class="flex items-center space-x-2">
                          <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                            Fällig Fr.
                          </span>
                          <input type="checkbox" class="w-4 h-4 text-orange-600 border-gray-300 rounded focus:ring-orange-500">
                        </div>
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
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-orange-400 hover:bg-orange-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-orange-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-orange-700">Hausaufgabe hinzufügen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-blue-400 hover:bg-blue-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-blue-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-blue-700">Note eintragen</span>
                </button>
                <button class="flex items-center space-x-3 p-4 border-2 border-gray-200 rounded-lg hover:border-green-400 hover:bg-green-50 transition-all group">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-green-600" aria-hidden="true" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M9 11H7v2h2v-2zm4 0h-2v2h2v-2zm4 0h-2v2h2v-2zm2-7h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V9h14v11z"/>
                  </svg>
                  <span class="text-sm font-medium text-gray-700 group-hover:text-green-700">Stundenplan anzeigen</span>
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
export class SchoolComponent {}