import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

type VerificationResult = string; 

@Injectable({
  providedIn: 'root', 
})
export class ChildAuthService {
  
  // כתובת הבסיס של הקונטרולר
  private apiUrl = 'https://localhost:7222/api/Child'; 
  
  constructor(private http: HttpClient) {}

  /**
   * שולח בקשת POST לנקודת הקצה C# 'verify'
   */
  verifyIdentity(idNumber: string, birthDate: string): Observable<VerificationResult> {
    
    const verificationData = {
      idNumber: idNumber,
      birthDate: birthDate 
    };

    // 🛑 התיקון הקריטי: הגדרת responseType כ'text'
    // זה מונע מ-Angular לנסות לפרסר JSON, ומקבל את התשובה כמחרוזת גולמית.
    // שים לב שצריך להסיר את `<VerificationResult>` מה-post כדי להתאים ל-'responseType: text'.
    return this.http.post(`${this.apiUrl}/verify`, verificationData, { responseType: 'text' }) as Observable<VerificationResult>;
  }
}
