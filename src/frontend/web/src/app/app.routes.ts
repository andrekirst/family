import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';

export const routes: Routes = [
  // Redirect root to dashboard
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  
  // Authentication routes (for guests only)
  {
    path: 'auth',
    canActivate: [GuestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'callback',
        loadComponent: () => import('./features/auth/callback/callback.component').then(m => m.CallbackComponent)
      },
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      }
    ]
  },
  
  // Protected routes (require authentication)
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/calendar/calendar.component').then(m => m.CalendarComponent)
      },
      {
        path: 'school',
        loadComponent: () => import('./features/school/school.component').then(m => m.SchoolComponent)
      },
      {
        path: 'kindergarten',
        loadComponent: () => import('./features/kindergarten/kindergarten.component').then(m => m.KindergartenComponent)
      },
      {
        path: 'health',
        loadComponent: () => import('./features/health/health.component').then(m => m.HealthComponent)
      },
      {
        path: 'family',
        loadComponent: () => import('./features/family/family.component').then(m => m.FamilyComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'developer',
        children: [
          {
            path: 'graphql-playground',
            loadComponent: () => import('./features/developer-tools/graphql-playground/graphql-playground.component').then(m => m.GraphqlPlaygroundComponent)
          },
          {
            path: 'health-dashboard',
            loadComponent: () => import('./features/developer-tools/health-dashboard/health-dashboard.component').then(m => m.HealthDashboardComponent)
          },
          {
            path: '',
            redirectTo: 'graphql-playground',
            pathMatch: 'full'
          }
        ]
      }
    ]
  },
  
  // Error pages
  {
    path: 'error',
    children: [
      {
        path: '404',
        loadComponent: () => import('./features/error-pages/not-found.component').then(m => m.NotFoundComponent)
      },
      {
        path: '500',
        loadComponent: () => import('./features/error-pages/server-error.component').then(m => m.ServerErrorComponent)
      },
      {
        path: '',
        redirectTo: '404',
        pathMatch: 'full'
      }
    ]
  },
  
  // Wildcard route - must be last
  { 
    path: '**', 
    loadComponent: () => import('./features/error-pages/not-found.component').then(m => m.NotFoundComponent)
  }
];
