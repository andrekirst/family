import { RouterModule, Routes } from '@angular/router';
import { InstallComponent } from './setup/install/install.component';
import { NgModule } from '@angular/core';

export const routes: Routes = [
    { path: 'setup/install', component: InstallComponent }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes)
    ],
    exports: [
        RouterModule
    ]
})
export class AppRoutingModule
{
}
