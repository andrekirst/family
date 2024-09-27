import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-install',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './install.component.html',
  styleUrl: './install.component.scss'
})
export class InstallComponent implements OnInit {
  form!: FormGroup<{
    firstName: FormControl<string>;
    lastName: FormControl<string>;
    email: FormControl<string>;
    username: FormControl<string>;
    password: FormControl<string>;
    }>;

 constructor(private formBuilder: FormBuilder) {
 }

  ngOnInit(): void {
    this.initForm();
    this.triggerValidation();
  }

  triggerValidation(): void {
    Object.keys(this.form.controls).forEach(field => {
      const control = this.form.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  initForm(): void {
    this.form = this.formBuilder.nonNullable.group({
      firstName: this.formBuilder.nonNullable.control(''),
      lastName: this.formBuilder.nonNullable.control(''),
      email: this.formBuilder.nonNullable.control(''),
      username: this.formBuilder.nonNullable.control(''),
      password: this.formBuilder.nonNullable.control('')
    });

    this.form.controls['firstName'].addValidators([Validators.required]);
    this.form.controls['lastName'].addValidators([Validators.required]);
    this.form.controls['email'].addValidators([Validators.required]);
    this.form.controls['username'].addValidators([Validators.required]);
    this.form.controls['password'].addValidators([Validators.required, Validators.minLength(8)]);
  }

  onSubmit() {
  }

  reset(): void {
    this.form.reset();
  }
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