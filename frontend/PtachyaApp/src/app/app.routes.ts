import { Routes } from '@angular/router';
// 1. ייבוא קומפוננטת ה-LOGIN של המנהל (הנתיב הישן)
import { LoginComponent as AdminLoginComponent } from './user/components/login/login';
// 2. ייבוא קומפוננטת ה-LOGIN של ההורה/ילד (הנתיב החדש בו עבדנו)
import { Login as ParentLoginComponent } from './child/components/login/login'; 
// 3. ייבוא קומפוננטות נוספות
import { DataUpdateComponent } from './user/components/data-update/data-update'; 
import { ChildrenData } from './user/components/children-data/children-data';
import { Main } from './child/components/main/main';
import { authGuard } from './auth-guard'; // ✅ ייבוא ה-Guard
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: ParentLoginComponent },
  
  { path: 'admin/login', component: AdminLoginComponent },

  { 
    path: 'data-update', 
    component: DataUpdateComponent, 
    canActivate: [authGuard] // ✅ יישום ה-Guard
  },
  { path: 'children-data', component: ChildrenData },
  { path: 'main/:username', component: Main }, 
];
