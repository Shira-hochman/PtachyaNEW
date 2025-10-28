// src/app/user/service/login.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest, LoginResponse } from '../../../models/login'; // ✅ נתיב עדכני

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private apiUrl = 'https://localhost:7222/api/User';


  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<LoginResponse> {
    const body: LoginRequest = { Username: username, PasswordHash: password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, body);
  }
}