// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './user/components/login/login'; // ✅ נתיב עדכני

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
];