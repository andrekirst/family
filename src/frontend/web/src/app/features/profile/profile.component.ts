import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto px-6 py-8 max-w-4xl">
      <!-- Profile Card -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden transform transition-all duration-300 hover:shadow-xl">
        <!-- Card Header -->
        <div class="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4">
          <div class="flex items-center space-x-4">
            <!-- Profile Icon -->
            <div class="flex-shrink-0">
              <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                <svg class="w-10 h-10 text-white" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                </svg>
              </div>
            </div>
            <!-- Title and Subtitle -->
            <div>
              <h1 class="text-2xl font-bold text-white">Benutzerprofil</h1>
              <p class="text-indigo-100 text-sm">Persönliche Informationen verwalten</p>
            </div>
          </div>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <div class="space-y-4">
            <p class="text-gray-700 text-base">
              Hier können Sie Ihre persönlichen Informationen bearbeiten.
            </p>
            <div class="bg-amber-50 border-l-4 border-amber-400 p-4 rounded">
              <div class="flex items-start">
                <svg class="w-5 h-5 text-amber-600 mt-0.5 mr-3 flex-shrink-0" aria-hidden="true" viewBox="0 0 24 24">
                  <path fill="currentColor" d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/>
                </svg>
                <p class="text-amber-700 text-sm">
                  Diese Funktion wird in einer zukünftigen Version implementiert.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Future Profile Form Placeholder -->
          <div class="mt-8 space-y-6">
            <!-- Basic Information Section -->
            <div class="border-t pt-6">
              <h2 class="text-lg font-semibold text-gray-900 mb-4">Grundinformationen</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="space-y-1">
                  <label class="text-sm font-medium text-gray-700">Vorname</label>
                  <div class="h-10 bg-gray-100 rounded-md animate-pulse"></div>
                </div>
                <div class="space-y-1">
                  <label class="text-sm font-medium text-gray-700">Nachname</label>
                  <div class="h-10 bg-gray-100 rounded-md animate-pulse"></div>
                </div>
                <div class="space-y-1">
                  <label class="text-sm font-medium text-gray-700">E-Mail</label>
                  <div class="h-10 bg-gray-100 rounded-md animate-pulse"></div>
                </div>
                <div class="space-y-1">
                  <label class="text-sm font-medium text-gray-700">Telefon</label>
                  <div class="h-10 bg-gray-100 rounded-md animate-pulse"></div>
                </div>
              </div>
            </div>
            
            <!-- Family Information Section -->
            <div class="border-t pt-6">
              <h2 class="text-lg font-semibold text-gray-900 mb-4">Familieninformationen</h2>
              <div class="space-y-3">
                <div class="flex items-center justify-between p-3 bg-gray-50 rounded-md">
                  <span class="text-sm text-gray-600">Familie</span>
                  <div class="h-5 w-32 bg-gray-200 rounded animate-pulse"></div>
                </div>
                <div class="flex items-center justify-between p-3 bg-gray-50 rounded-md">
                  <span class="text-sm text-gray-600">Rolle</span>
                  <div class="h-5 w-24 bg-gray-200 rounded animate-pulse"></div>
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
export class ProfileComponent {}