import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ⭐️ תיקון הממשק (Interface) כדי שיתאים לנתונים השטוחים מהשרת ⭐️
export interface FormDto {
  formId: number;
  formType: string; // 'HEALTH_DECLARATION' | 'DISCOUNT_REQUEST'
  status: string;   // 'Pending' | 'Approved'
  submittedDate: string; // מגיע כ-string
  filePath: string;      // הלינק המלא לקובץ
  
  // 🛑 הוסר האובייקט המקונן: child?: { ... }
  
  // ✅ הוספו השדות השטוחים שהשרת שולח עכשיו:
  childFirstName: string;
  childLastName: string;
  childIdNumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class FormDataService {
  // כתובת ה-API של הקונטרולר
  private apiUrl = 'https://localhost:7222/api/Form'; 

  constructor(private http: HttpClient) { }

  /**
   * מחזיר את רשימת כל הטפסים הממתינים לאישור (סטטוס Pending)
   */
  getPendingForms(): Observable<FormDto[]> {
    return this.http.get<FormDto[]>(`${this.apiUrl}/pending`);
  }

  getApprovedForms(): Observable<FormDto[]> {
    return this.http.get<FormDto[]>(`${this.apiUrl}/approved`);
  }

  /**
   * מאשר טופס ספציפי לפי ID
   */
  approveForm(formId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/approve/${formId}`, {});
  }
}