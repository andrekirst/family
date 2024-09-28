import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Client, InstallationOptions } from '../../api-client/api-client';

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
    const client = new Client("https://localhost:7076");
    const options = new InstallationOptions({
      firstName: this.form.get('firstName')?.value,
      lastName: this.form.get('lastName')?.value,
      email: this.form.get('email')?.value,
      username: this.form.get('username')?.value,
      password: this.form.get('password')?.value
    })
    
    client.install(options);
  }

  reset(): void {
    this.form.reset();
  }
}
