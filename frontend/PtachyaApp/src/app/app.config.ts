// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes'; 
import { provideHttpClient } from '@angular/common/http';
// ⚠️ אין צורך לייבא כאן את FormsModule, הוא מיובא ברכיב ה-Login עצמו

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), 
    provideHttpClient() // ⬅️ חובה לטובת תקשורת רשת
  ]
};