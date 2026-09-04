import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
export interface ChildDto {
  childId: number; 
  kindergartenId: number;
  kindergartenName: string;
  idNumber: string;
  birthDate: Date;
  firstName: string;
  lastName: string;
  schoolYear: string;
  formLink: string;
  phone: string;
  email: string;
  paymentId: number;
}

export interface PagedChildResult {
  items: ChildDto[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ChildDataService {
  // כתובת בסיס לילדים
  private apiUrl = `${environment.apiBaseUrl}/api/Child`; 
  
  // ✅ כתובת בסיס לגנים (בהתאם לקונטרולר ששלחת)
  private kindergartenUrl = `${environment.apiBaseUrl}/api/Kindergarten`;

  constructor(private http: HttpClient) { }

  getAllChildren(): Observable<ChildDto[]> {
    return this.http.get<ChildDto[]>(this.apiUrl);
  }

  // ✅ הפונקציה הזו כעת פונה לקונטרולר הקיים שלך!
  getAllKindergartens(): Observable<any[]> {
    return this.http.get<any[]>(this.kindergartenUrl);
  }

 getChildrenPaged(
  page: number,
  pageSize: number,
  searchTerm?: string,
  kindergartenId?: number | null,
  schoolYear?: string // <-- הוספת הפרמטר
) {
  const params: any = { page, pageSize };
  if (searchTerm) params.searchTerm = searchTerm;
  if (kindergartenId) params.kindergartenId = kindergartenId;
  if (schoolYear) params.schoolYear = schoolYear; // <-- שליחה לשרת

  return this.http.get<any>(`${this.apiUrl}/paged`, { params });
}

  updateChild(child: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${child.childId}`, child);
  }
}