import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  // כתובת השרת שלך (ה-Backend) - בענן זו תהיה הכתובת המלאה
  private apiUrl = 'https://localhost:7222/api/payments';

  constructor(private http: HttpClient) {}

  submitToBackend(paymentData: any): Observable<any> {
    // שליחת הנתונים לשרת שלך בצורה מאובטחת
    return this.http.post(`${this.apiUrl}/process`, paymentData);
  }
}