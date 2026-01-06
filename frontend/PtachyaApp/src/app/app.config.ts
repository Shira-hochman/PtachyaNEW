// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes'; 
import { provideHttpClient, withInterceptors } from '@angular/common/http'; 
import { authInterceptor } from './auth-interceptor'; 
// 👇 1. הייבוא שחסר לך
import { provideAnimations } from '@angular/platform-browser/animations'; 

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), 
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    // 👇 2. הפונקציה שמפעילה את האנימציות (קריטי ל-PrimeNG)
    provideAnimations() 
  ]
};