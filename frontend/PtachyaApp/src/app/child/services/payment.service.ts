import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  // כתובת השרת שלך (ה-Backend) - בענן זו תהיה הכתובת המלאה
  private apiUrl = `${environment.apiBaseUrl}/api/payments`;

  constructor(private http: HttpClient) {}

  submitToBackend(paymentData: any): Observable<any> {
    // שליחת הנתונים לשרת שלך בצורה מאובטחת
    return this.http.post(`${this.apiUrl}/process`, paymentData);
  }
}