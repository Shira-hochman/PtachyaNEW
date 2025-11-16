// services/child-auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs'; // ⭐️ ייבוא tap לשמירת המצב
import { Child } from '../../../models/child'; // ייבוא המודל

interface AuthResponse {
  token: string;
  child: Child;
}

@Injectable({
  providedIn: 'root', 
})
export class ChildAuthService {
    
  private apiUrl = 'https://localhost:7222/api/Child'; 
  private childKey = 'current_child_data'; // מפתח לשמירת פרטי הילד ב-LocalStorage
  private tokenKey = 'auth_token'; // מפתח לשמירת התוקן
  
  // ⭐️ 1. המשתנה שיחזיק את נתוני הילד לאחר אימות מוצלח
  private currentChild: Child | null = null; 

 constructor(private http: HttpClient) {
    // ⭐️ טעינת נתוני הילד בזמן אתחול השירות אם קיימים
    const storedChild = localStorage.getItem(this.childKey);
    if (storedChild) {
        // ... (ניתן לטעון את הילד או פשוט להסתמך על הטוקן)
    }
  }

  /**
   * ⭐️ 2. הפונקציה המרכזית: מאמתת את הילד ושולפת את כל פרטיו.
   * שמה הוחלף ל-getChildDetails כדי לשקף את התוצאה המלאה.
   */
  public getChildDetails(idNumber: string, birthDate: string): Observable<AuthResponse> {
      const verificationData = {
          idNumber: idNumber,
          birthDate: birthDate 
      };

      // ⭐️ 1. שינוי לקריאה ל-login
      return this.http.post<AuthResponse>(`${this.apiUrl}/login`, verificationData)
        .pipe(
          // ⭐️ 2. שמירת התוקן ופרטי הילד לאחר קבלה מוצלחת
          tap(response => {
                localStorage.setItem(this.tokenKey, response.token);
                // שומרים את נתוני הילד המלאים כדי שיהיו זמינים לטפסים
                localStorage.setItem(this.childKey, JSON.stringify(response.child)); 
           })
        );
  }

  public getCurrentChild(): Child | null {
    const storedChild = localStorage.getItem(this.childKey);
    // ⭐️ 3. שולפים את נתוני הילד מ-LocalStorage
    return storedChild ? JSON.parse(storedChild) : null;
  }
  
  public getAuthToken(): string | null {
    // ⭐️ 4. פונקציה לשליפת התוקן (תשמש ב-Interceptor)
    return localStorage.getItem(this.tokenKey);
  }
  
  public isLoggedIn(): boolean {
    // ⭐️ 5. בודק אם קיים טוקן (אימות בסיסי)
    return !!this.getAuthToken(); 
  }
  
  public logout(): void {
    // ⭐️ 6. ניקוי ה-LocalStorage וניתוב לדף הכניסה
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.childKey);
    // יש לנתב את המשתמש לדף הכניסה
  }
}