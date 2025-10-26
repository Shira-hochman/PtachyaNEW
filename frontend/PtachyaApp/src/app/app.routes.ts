// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './user/components/login/login'; // ✅ נתיב עדכני
import { DataUpdateComponent } from './user/components/data-update/data-update'; // ⬅️ וודא ייבוא נכון
import{ ChildrenData }from './user/components/children-data/children-data';
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'data-update/:username', component: DataUpdateComponent },
  { path: 'children-data', component: ChildrenData },
];