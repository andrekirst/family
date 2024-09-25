import { Component } from '@angular/core';

@Component({
  selector: 'app-install',
  standalone: true,
  imports: [],
  templateUrl: './install.component.html',
  styleUrl: './install.component.scss'
})
export class InstallComponent {

}


/* Service:
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'https://deine-api-url.com/endpoint';

  constructor(private http: HttpClient) { }

  sendForm(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }
}

 */

/**
 * import { Component } from '@angular/core';
import { ApiService } from './api.service';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html'
})
export class FormComponent {
  formData = {
    // Deine Formulardaten hier
  };

  constructor(private apiService: ApiService) { }

  onSubmit() {
    this.apiService.sendForm(this.formData).subscribe(response => {
      console.log('Formular erfolgreich gesendet', response);
    }, error => {
      console.error('Fehler beim Senden des Formulars', error);
    });
  }
}

 */

/*

<form (ngSubmit)="onSubmit()">
  <!-- Deine Formularfelder hier -->
  <button type="submit" class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded">
    Senden
  </button>
</form>

* */