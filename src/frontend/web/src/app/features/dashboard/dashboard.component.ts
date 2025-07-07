import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { User } from '../../core/graphql/types';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  currentUser$: Observable<User | null>;

  private authService = inject(AuthService);

  constructor() {
    this.currentUser$ = this.authService.currentUser$;
  }
}