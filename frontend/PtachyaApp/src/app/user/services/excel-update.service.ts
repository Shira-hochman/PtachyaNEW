// excel-update.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

// הגדרת interface זמני עבור האפשרויות הנדרשות
interface TextResponseOptions {
    headers?: HttpHeaders | { [header: string]: string | string[]; };
    context?: HttpContext;
    params?: HttpParams | { [param: string]: string | number | boolean | readonly (string | number | boolean)[]; };
    reportProgress?: boolean;
    observe?: 'body'; // אנחנו רוצים רק את הגוף
    withCredentials?: boolean;
    responseType: 'text'; // 🚨 חשוב: responseType חייב להיות 'text'
}

@Injectable({
  providedIn: 'root'
})
export class ExcelUpdateService {
  private apiUrl = 'https://localhost:7222/api/Import'; 

  constructor(private http: HttpClient) {}

  /**
   * שולח קובץ Excel לשרת לעיבוד נתוני ילדים.
   * נקודת קצה: POST /api/Import/children
   */
  uploadChildren(file: File): Observable<string> {
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);

    const options: TextResponseOptions = { responseType: 'text' };

    // 🚨 הפתרון: קוראים ל-post ומכריחים את התוצאה להיות Observable<string>
    return this.http.post(`${this.apiUrl}/children`, formData, options) as Observable<string>;
  }
  
  /**
   * שולח קובץ Excel לשרת לעיבוד נתוני גנים.
   * נקודת קצה: POST /api/Import/kindergarten/excel
   */
  uploadGardens(file: File): Observable<string> {
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);
    
    const options: TextResponseOptions = { responseType: 'text' };

    // 🚨 הפתרון: קוראים ל-post ומכריחים את התוצאה להיות Observable<string>
    return this.http.post(`${this.apiUrl}/kindergarten/excel`, formData, options) as Observable<string>;
  }
}