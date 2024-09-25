import { RouterModule, Routes } from '@angular/router';
import { InstallComponent } from './setup/install/install.component';
import { NgModule } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app.component';

export const routes: Routes = [
    { path: 'setup/install', component: InstallComponent }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes)
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
