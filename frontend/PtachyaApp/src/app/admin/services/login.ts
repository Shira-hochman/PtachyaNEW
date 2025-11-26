import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router'; 
import { LoginRequest, LoginResponse } from '../../../models/login'; 

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private apiUrl = 'https://localhost:7222/api/User';

  constructor(private http: HttpClient, private router: Router) { } 

  login(username: string, password: string): Observable<LoginResponse> {
    const body: LoginRequest = { Username: username, PasswordHash: password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, body).pipe(
      tap(res => {
        if (res.isSuccess && res.token) {
          // 1. שמירת הטוקן
          localStorage.setItem('auth_token', res.token);
          localStorage.setItem('username', username); // נשמור את השם לשימוש מיידי
          
          // 2. ❌ תיקון: ניתוב לאזור המנהל הראשי (לא data-update)
          // ה-Component עצמו שקורא ל-login יטפל בניווט, אבל נעדכן את ה-localStorage
        }
      })
    );
  }

  /**
   * ⭐️⭐️⭐️ הוספה: שליפת שם המשתמש המחובר ⭐️⭐️⭐️
   */
  getCurrentUsername(): string | null {
    // ניתן לשלוף ישירות מה-localStorage לאחר שמירה ב-login
    return localStorage.getItem('username');
    
    // ביישום מלא: היינו מפענחים את הטוקן JWT כדי להיות בטוחים
  }

  // 🚪 מתודת יציאה: ניקוי האסימון והניתוב
  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('username');
    this.router.navigate(['/admin/login']);
  }
  
  // ℹ️ מתודה לבדיקת סטטוס חיבור
  isLoggedIn(): boolean {
    return !!localStorage.getItem('auth_token');
  }
}