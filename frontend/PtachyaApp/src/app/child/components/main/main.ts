import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './main.html', // ודאי שהשמות תואמים לקבצים שלך
  styleUrls: ['./main.css'],
})
export class Main implements OnInit {
  
  // משתנה לשם המשתמש (ניתן לשלוף אותו מה-Service בהמשך)
  userName: string = 'הורה/ילד יקר'; 
  currentDate: string = '';

  constructor(private router: Router) {}

  ngOnInit(): void {
    // הגדרת התאריך הנוכחי לתצוגה
    const now = new Date();
    this.currentDate = now.toLocaleDateString('he-IL', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  /**
   * ניווט לטופס הצהרת בריאות
   */
  onButton1Click(): void {
    this.router.navigate(['/child/health-declaration']);
  }

  /**
   * ניווט לטופס התשלום
   */
  onButton2Click(): void {
    this.router.navigate(['/child/payment-form']);
  }
  
  logout(): void {
      // כאן תוסיפי בעתיד לוגיקת התנתקות
      this.router.navigate(['/login']);
  }
}