import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// DTOs (מבוסס על מודל Kindergarten ב-C#)
export interface KindergartenDto {
  kindergartenId: number;
  code: string;
  name: string;
  address: string;
}

// DTO לעדכון (בדומה ל-KindergartenDto, אך ללא ID אם זה הוספה)
export interface UpsertKindergartenDto {
  code: string;
  name: string;
  address: string;
}

@Injectable({
  providedIn: 'root'
})
export class GardenDataService {
  // ⚠️ יש לוודא שה-Base URL תואם לשרת שלך
  private apiUrl = 'https://localhost:7222/api/Kindergarten'; 

  constructor(private http: HttpClient) { }

  /**
   * שולף את רשימת כל הגנים
   */
  getAllGardens(): Observable<KindergartenDto[]> {
    return this.http.get<KindergartenDto[]>(this.apiUrl);
  }

  /**
   * יוצר גן חדש
   */
  addGarden(dto: UpsertKindergartenDto): Observable<KindergartenDto> {
    return this.http.post<KindergartenDto>(this.apiUrl, dto);
  }

  /**
   * מעדכן גן קיים
   */
  updateGarden(id: number, dto: UpsertKindergartenDto): Observable<KindergartenDto> {
    return this.http.put<KindergartenDto>(`${this.apiUrl}/${id}`, dto);
  }

  /**
   * מוחק גן לפי ID
   */
  deleteGarden(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}