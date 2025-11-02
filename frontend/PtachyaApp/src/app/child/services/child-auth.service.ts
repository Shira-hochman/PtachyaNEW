// services/child-auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs'; // ⭐️ ייבוא tap לשמירת המצב
import { Child } from '../../../models/child'; // ייבוא המודל

@Injectable({
  providedIn: 'root', 
})
export class ChildAuthService {
    
  private apiUrl = 'https://localhost:7222/api/Child'; 
  
  // ⭐️ 1. המשתנה שיחזיק את נתוני הילד לאחר אימות מוצלח
  private currentChild: Child | null = null; 

  constructor(private http: HttpClient) {}

  /**
   * ⭐️ 2. הפונקציה המרכזית: מאמתת את הילד ושולפת את כל פרטיו.
   * שמה הוחלף ל-getChildDetails כדי לשקף את התוצאה המלאה.
   */
  getChildDetails(idNumber: string, birthDate: string): Observable<Child> {
      
      const verificationData = {
          idNumber: idNumber,
          birthDate: birthDate 
      };


      return this.http.post<Child>(`${this.apiUrl}/details`, verificationData)
        .pipe(
          // ⭐️ 3. שומרים את האובייקט המלא בזיכרון המקומי של ה-Service
          tap(child => this.currentChild = child)
        );
  }

  /**
   * 4. מאפשר לקומפוננטות אחרות לגשת לנתונים השמורים ב-Service.
   */
  public getCurrentChild(): Child | null {
    return this.currentChild;
  }
}