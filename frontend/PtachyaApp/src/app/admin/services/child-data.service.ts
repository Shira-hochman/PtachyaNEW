import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// 🚨 מבנה ChildDto מעודכן עם כל השדות שציינת
export interface ChildDto {
  childId: number; 
  kindergartenId: string;
  idNumber: string;
  birthDate: Date; // או string, תלוי איך ה-C# שולח
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
  // הנתיב: ודא שהוא מצביע לקונטרולר הנכון ב-C# (לרוב /api/Child)
  private apiUrl = 'https://localhost:7222/api/Child'; 

  constructor(private http: HttpClient) { }

  /**
   * מחזיר את רשימת כל הילדים.
   * נתיב: GET /api/Child
   */
  getAllChildren(): Observable<ChildDto[]> {
    // מצפה לרשימה של ChildDto בפורמט JSON
    return this.http.get<ChildDto[]>(this.apiUrl);
  }
  // עדכן את הממשק או הוסף חדש


// בתוך המחלקה:
getChildrenPaged(page: number, pageSize: number): Observable<PagedChildResult> {
  // שים לב לשינוי הנתיב ל-paged והוספת הפרמטרים
  return this.http.get<PagedChildResult>(`${this.apiUrl}/paged?page=${page}&pageSize=${pageSize}`);
}
}
