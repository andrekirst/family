import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApolloConfigService {
  
  private readonly http = inject(HttpClient);

  getUri(): string {
    return environment.apiUrl;
  }

  getHeaders(): { [key: string]: string } {
    const headers: { [key: string]: string } = {};
    
    const token = localStorage.getItem('accessToken');
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    return headers;
  }
}