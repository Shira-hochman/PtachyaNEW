// src/app/app.config.ts

import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http'; // 👈 הוספה זו

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // הוספת ספק ה-HTTP כדי לאפשר תקשורת עם ה-C#
    provideHttpClient() // 👈 חובה: מאפשר שימוש ב-HttpClient
  ]
};