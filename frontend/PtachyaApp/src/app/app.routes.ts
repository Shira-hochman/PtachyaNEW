import { Routes } from '@angular/router';

// 1. ייבוא קומפוננטת ה-LOGIN של המנהל (הנתיב הישן)
import { LoginComponent as AdminLoginComponent } from './user/components/login/login';

// 2. ייבוא קומפוננטת ה-LOGIN של ההורה/ילד (הנתיב החדש בו עבדנו)
import { Login as ParentLoginComponent } from './child/components/login/login'; 

// 3. ייבוא קומפוננטות נוספות
import { DataUpdateComponent } from './user/components/data-update/data-update'; 
import { ChildrenData } from './user/components/children-data/children-data';

export const routes: Routes = [
  // ניתוב ברירת מחדל
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  
  // 🅰️ מסלול כניסת ההורה/לקוח (משתמש בקומפוננטה החדשה)
  { path: 'login', component: ParentLoginComponent },
  
  // 🅱️ מסלול כניסת המנהל (משתמש בקומפוננטה ששמה שונה ל-AdminLoginComponent)
  { path: 'admin/login', component: AdminLoginComponent },

  // ניתובים מוגנים לאחר התחברות
  { path: 'data-update/:username', component: DataUpdateComponent },
  { path: 'children-data', component: ChildrenData },
];
