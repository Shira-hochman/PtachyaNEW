// src/app/services/form-data.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ⭐️ ממשקי DTO ⭐️
export interface FormDto {
  formId: number;
  formType: string; // 'HEALTH_DECLARATION' | 'DISCOUNT_REQUEST'
  status: string;   // 'Pending' | 'Approved'
  submittedDate: string; // מגיע כ-string
  filePath: string;      // הלינק המלא לקובץ
  
  childFirstName: string;
  childLastName: string;
  childIdNumber: string;
}
export interface ChildFormDto {
  formId: number;
  formType: string;
  fileName: string;
  downloadUrl: string; // הלינק המלא להורדה
  uploadDate: string;
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
  
  getFormsByIdNumber(idNumber: string): Observable<ChildFormDto[]> {
    return this.http.get<ChildFormDto[]>(`${this.apiUrl}/by-id-number/${idNumber}`);
  }

  // ⭐️⭐️⭐️ פונקציה חדשה: הורדה מאובטחת באמצעות HttpClient ⭐️⭐️⭐️
  downloadFileByUrl(url: string): Observable<Blob> {
      // responseType: 'blob' חיוני לקבלת קובץ בינארי
      // ה-Interceptor יוסיף את הטוקן באופן אוטומטי לבקשה זו.
      return this.http.get(url, { responseType: 'blob' });
  }
}