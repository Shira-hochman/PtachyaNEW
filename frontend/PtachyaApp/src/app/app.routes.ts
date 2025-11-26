import { Routes } from '@angular/router';

// ----------------------------------------------------------------------
// 🚨 ייבוא קומפוננטות הניהול החדשות (שימוש במבנה שלב קודם)
// ----------------------------------------------------------------------
import { AdminLayoutComponent } from './admin/components/admin-layout/admin-layout';
import { NavbarComponent } from './admin/components/navbar/navbar';
import { SidebarComponent } from './admin/components/sidebar/sidebar';
import { DashboardComponent } from './admin/components/dashboard/dashboard';
import { ChildrenManagementComponent } from './admin/components/children-management/children-management';

// ----------------------------------------------------------------------
// 1. ייבוא קומפוננטות ניהול (Admin Modules)
// ----------------------------------------------------------------------
import { LoginComponent as AdminLoginComponent } from './admin/components/login/login';
import { DataUpdateComponent } from './admin/components/data-update/data-update'; 

// ----------------------------------------------------------------------
// 2. ייבוא קומפוננטות משתמש (Parent/Child Modules)
// ----------------------------------------------------------------------
import { LoginComponent as ParentLoginComponent } from './child/components/login/login'; 
import { Main } from './child/components/main/main';
import { HealthDeclarationComponent } from './child/components/health-declaration/health-declaration';
import { PaymentForm } from './child/components/payment-form/payment-form'; 

// ----------------------------------------------------------------------
// 3. ייבוא Guard
// ----------------------------------------------------------------------
import { authGuard } from './auth-guard'; 


export const routes: Routes = [
    // ------------------------------------------------------------------
    // 🏠 נתיבים ראשיים (User/Parent Routes)
    // ------------------------------------------------------------------
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', component: ParentLoginComponent },
    
    // נתיבים הדורשים התחברות של ההורה
    { 
        path: 'child', // נתיב מעטפת כללי לכל פעולות ההורה/ילד
         // רכיב Main יכול לשמש כמעטפת פנימית
        canActivate: [authGuard], 
        children: [
            // הוספת נתיב ראשי בתוך המעטפת אם Main אינו דף ספציפי
            { path: 'main', component: Main }, 
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }, 
            
            // טפסים ופעולות
            { path: 'health-declaration', component: HealthDeclarationComponent },
            { path: 'payment-form', component: PaymentForm },
        ]
    },
    
    // ------------------------------------------------------------------
    // 🔑 נתיבי ניהול (Admin Routes)
    // ------------------------------------------------------------------
    { path: 'admin/login', component: AdminLoginComponent },

    { 
        path: 'admin', 
        component: AdminLayoutComponent, // ⬅️ המעטפת הניהולית החדשה שלנו
        // 💡 ניתן להוסיף Guard ייעודי למנהל כאן
        canActivate: [authGuard], 
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            
            // 1. לוח מחוונים
            { path: 'dashboard', component: DashboardComponent },
            
            // 2. ניהול ילדים
            { 
                path: 'children', 
                component: ChildrenManagementComponent // ⬅️ ניהול הילדים המשודרג
                // הנתיב הישן 'children-data' כבר לא קיים, הוא בתוך המעטפת החדשה 
            },
            
            // 3. עדכון נתונים
            { path: 'update-data', component: DataUpdateComponent },
            
            // 4. נתיבים נוספים שנגדיר בהמשך
            // { path: 'gardens', component: GardensManagementComponent },
            // { path: 'forms', component: FormsManagementComponent },
            // { path: 'payments', component: PaymentsManagementComponent },
        ]
    },

    // ------------------------------------------------------------------
    // 🛑 נתיב שגיאה (404)
    // ------------------------------------------------------------------
    { path: '**', redirectTo: '' }
];