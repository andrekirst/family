import { RouterModule, Routes } from '@angular/router';
import { InstallComponent } from './setup/install/install.component';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthConfig, OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { StateHandlerService, StateHandlerServiceImpl } from './services/state-handler.service';
import { StateHandlerProcessorService, StateHandlerProcessorServiceImpl } from './services/state-handler-processor.service';
import { StorageService } from './services/storage.service';

export const routes: Routes = [
    { path: 'setup/install', component: InstallComponent }
];

const domain = "http://localhost:420";

const authConfig: AuthConfig = {
    scope: "openid profile email offline_access",
    responseType: "code",
    oidc: true,
    clientId: "284380841054896131",
    issuer: "https://localhost:8081",
    redirectUri: `${domain}/auth/callback`,
    postLogoutRedirectUri: `${domain}/signedout`,
    requireHttps: false
}

const stateHandlerFunction = (stateHandler: StateHandlerService) => {
    return () => {
        return stateHandler.initStateHandler();
    }
};

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forRoot(routes),
        ReactiveFormsModule,
        FormsModule,
        OAuthModule.forRoot({
            resourceServer: {
                allowedUrls: [`${domain}/admin/v1`, `${domain}/management/v1`, `${domain}/auth/v1/`],
                sendAccessToken: true
            }
        })
    ],
    exports: [
        RouterModule
    ],
    providers: [
        {
            provide: APP_INITIALIZER,
            useFactory: stateHandlerFunction,
            multi: true,
            deps: [
                StateHandlerService
            ]
        },
        provideHttpClient(),
        {
            provide: AuthConfig,
            useValue: authConfig
        },
        {
            provide: StateHandlerProcessorService,
            useClass: StateHandlerProcessorServiceImpl
        },
        {
            provide: StateHandlerService,
            useClass: StateHandlerServiceImpl
        },
        {
            provide: OAuthStorage,
            useClass: StorageService
        }
    ]
})
export class AppRoutingModule
{
}
