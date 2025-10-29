import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router'; // ✅ ייבוא Router לשם ה-logout
import { LoginRequest, LoginResponse } from '../../../models/login'; // ⚠️ ודא נתיב נכון!

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private apiUrl = 'https://localhost:7222/api/User';

  constructor(private http: HttpClient, private router: Router) { } // ✅ הוספת Router

  login(username: string, password: string): Observable<LoginResponse> {
    const body: LoginRequest = { Username: username, PasswordHash: password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, body).pipe(
      tap(res => {
        if (res.isSuccess && res.token) {
          // 1. שמירת הטוקן
          localStorage.setItem('auth_token', res.token);
          localStorage.setItem('username', username);
          
          // 2. ניתוב (כאן ה-Component בדרך כלל מטפל בניווט, אבל נשאיר כאן ניווט לדוגמה)
          this.router.navigate(['/data-update', username]); 
        }
      })
    );
  }

  // 🚪 מתודת יציאה: ניקוי האסימון והניתוב
  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('username');
    this.router.navigate(['/login']);
  }
  
  // ℹ️ מתודה לבדיקת סטטוס חיבור
  isLoggedIn(): boolean {
    return !!localStorage.getItem('auth_token');
  }
}
