import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import {
  provideHttpClient,
  withInterceptors,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor.new';
import { ShowService } from './services/show.service';
import { AuthService } from './services/auth.service';
import { provideAnimations } from '@angular/platform-browser/animations';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
    ShowService,
    AuthService,
  ],
};
