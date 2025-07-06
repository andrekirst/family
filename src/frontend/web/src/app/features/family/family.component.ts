import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-family',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-7xl">
      <!-- Family Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-purple-600 to-pink-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- Family Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">{{ 'family.management.title' | translate }}</h1>
              <p class="text-purple-100 text-sm">{{ 'family.management.subtitle' | translate }}</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              {{ 'family.management.description' | translate }}
            </p>
            <div class="bg-purple-50 border-l-4 border-purple-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-purple-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
                <p class="text-purple-700 text-sm">
                  {{ 'family.management.notImplemented' | translate }}
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future Family Members Section -->
          <div class="mt-8">
            <!-- Family Overview -->
            <div class="mb-8">
              <h2 class="text-xl font-semibold text-gray-900 mb-4">{{ 'family.overview' | translate }}</h2>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="bg-gray-50 rounded-lg p-4">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium text-gray-600">{{ 'family.overview.members' | translate }}</span>
                    <svg class="w-5 h-5 text-gray-400" aria-hidden="true" viewBox="0 0 24 24">
                      <path fill="currentColor" d="M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z"/>
                    </svg>
                  </div>
                  <div class="h-8 bg-gray-200 rounded animate-pulse"></div>
                </div>
                <div class="bg-gray-50 rounded-lg p-4">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium text-gray-600">{{ 'family.overview.children' | translate }}</span>
                    <svg class="w-5 h-5 text-gray-400" aria-hidden="true" viewBox="0 0 24 24">
                      <path fill="currentColor" d="M14.5 10.5c0 1.38-1.12 2.5-2.5 2.5s-2.5-1.12-2.5-2.5 1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5zM12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-1.19 0-2.34-.26-3.33-.72.97-.92 2.27-1.48 3.83-1.48.21 0 .41.02.61.05.21-.06.4-.15.56-.29l.55-.55c.4-.39.4-1.02 0-1.41-.39-.4-1.02-.4-1.41 0l-.39.39c-.15.15-.25.34-.29.55-.56-.06-1.12-.1-1.62-.1-2.38 0-4.41.86-5.69 2.14C4.64 17.88 4 16.01 4 14c0-4.41 3.59-8 8-8s8 3.59 8 8-3.59 8-8 8z"/>
                    </svg>
                  </div>
                  <div class="h-8 bg-gray-200 rounded animate-pulse"></div>
                </div>
                <div class="bg-gray-50 rounded-lg p-4">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium text-gray-600">{{ 'family.overview.administrators' | translate }}</span>
                    <svg class="w-5 h-5 text-gray-400" aria-hidden="true" viewBox="0 0 24 24">
                      <path fill="currentColor" d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z"/>
                    </svg>
                  </div>
                  <div class="h-8 bg-gray-200 rounded animate-pulse"></div>
                </div>
              </div>
            </div>
            
            <!-- Family Members List -->
            <div>
              <h2 class="text-xl font-semibold text-gray-900 mb-4">{{ 'family.members.title' | translate }}</h2>
              <div class="space-y-4">
                <!-- Member Card 1 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-center space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-12 h-12 bg-purple-100 rounded-full flex items-center justify-center">
                        <svg class="w-6 h-6 text-purple-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="h-5 bg-gray-200 rounded w-1/3 mb-2 animate-pulse"></div>
                      <div class="h-4 bg-gray-200 rounded w-1/4 animate-pulse"></div>
                    </div>
                    <div class="flex-shrink-0">
                      <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                        {{ 'family.members.role.admin' | translate }}
                      </span>
                    </div>
                  </div>
                </div>
                
                <!-- Member Card 2 -->
                <div class="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div class="flex items-center space-x-4">
                    <div class="flex-shrink-0">
                      <div class="w-12 h-12 bg-pink-100 rounded-full flex items-center justify-center">
                        <svg class="w-6 h-6 text-pink-600" aria-hidden="true" viewBox="0 0 24 24">
                          <path fill="currentColor" d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                        </svg>
                      </div>
                    </div>
                    <div class="flex-1">
                      <div class="h-5 bg-gray-200 rounded w-1/3 mb-2 animate-pulse"></div>
                      <div class="h-4 bg-gray-200 rounded w-1/4 animate-pulse"></div>
                    </div>
                    <div class="flex-shrink-0">
                      <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                        {{ 'family.members.role.member' | translate }}
                      </span>
                    </div>
                  </div>
                </div>
                
                <!-- Add Member Button -->
                <button class="w-full border-2 border-dashed border-gray-300 rounded-lg p-4 hover:border-purple-400 transition-colors group">
                  <div class="flex items-center justify-center space-x-2 text-gray-600 group-hover:text-purple-600">
                    <svg class="w-5 h-5" aria-hidden="true" viewBox="0 0 24 24">
                      <path fill="currentColor" d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z"/>
                    </svg>
                    <span class="text-sm font-medium">{{ 'family.members.addMember' | translate }}</span>
                  </div>
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
export class FamilyComponent {}