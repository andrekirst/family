import { RouterModule, Routes } from '@angular/router';
import { InstallComponent } from './setup/install/install.component';
import { NgModule } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

export const routes: Routes = [
    { path: 'setup/install', component: InstallComponent }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forRoot(routes),
        ReactiveFormsModule,
        FormsModule
    ],
    exports: [
        RouterModule
    ],
    providers: [
        provideHttpClient()
    ]
})
export class AppRoutingModule
{
}
