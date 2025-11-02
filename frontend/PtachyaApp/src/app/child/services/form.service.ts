import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FormService {
  private apiUrl = 'https://localhost:7222/api/Form'; 

  constructor(private http: HttpClient) { }

  submitHealthDeclaration(formData: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/submit-health-declaration`, formData, {
      responseType: 'blob' // ⭐️ הכרחי לקבלת קובץ
    });
  }
}