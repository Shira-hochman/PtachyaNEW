import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FormService {
  // ⚠️ יש להחליף ב-URL האמיתי של ה-API
  private apiUrl = 'https://localhost:7222/api/Form'; 

  private http = inject(HttpClient);

  // ⭐️ חתימה קיימת: הצהרת בריאות
  submitHealthDeclaration(formData: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/submit-health-declaration`, formData, {
      responseType: 'blob' // חובה לציין לקבלת קובץ בינארי
    });
  }
  
  // ⭐️⭐️⭐️ מתודה חדשה: בקשת הנחה ⭐️⭐️⭐️
  submitDiscountRequest(formData: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/submit-discount-request`, formData, {
      responseType: 'blob' // חובה לקבלת קובץ PDF
    });
  }
}