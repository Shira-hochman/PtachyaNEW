import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
export interface AdminDto {
  userId: number;
  username: string;
}

export interface CreateAdminDto {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  
  // הכתובת של הקונטרולר שלך
  private apiUrl = `${environment.apiBaseUrl}/api/User`; 

  constructor(private http: HttpClient) { }

  // 1. שליפת כל המנהלים
  getAllAdmins(): Observable<AdminDto[]> {
    return this.http.get<AdminDto[]>(this.apiUrl);
  }

  // 2. הוספת מנהל חדש (שולח שם וסיסמה גלויה, השרת יצפין)
  addAdmin(admin: CreateAdminDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-admin`, admin);
  }

  // מחיקת מנהל לפי ID
deleteAdmin(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/${id}`);
}
}