import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FormService {
  // ⚠️ יש להחליף ב-URL האמיתי של ה-API (נניח שזה https://localhost:7222/api/Form)
  private apiUrl = 'https://localhost:7222/api/Form';

  private http = inject(HttpClient);

  // ⭐️ חתימה קיימת: הצהרת בריאות
  submitHealthDeclaration(formData: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/submit-health-declaration`, formData, {
      responseType: 'blob'
    });
  }

  // ⭐️⭐️⭐️ מתודה חדשה: בקשת הנחה (כעת מקבלת FormData) ⭐️⭐️⭐️
  // ה-URL תוקן: הסרנו את /Form/ המיותר והוספנו את הטיפוס FormData במקום any
  submitDiscountRequest(formData: FormData): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/submit-discount-request`, formData, {
      responseType: 'blob', // חובה לקבלת קובץ PDF
    });
  }
}