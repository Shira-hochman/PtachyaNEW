import { Component } from '@angular/core';
import { Router } from '@angular/router'; // 1. ייבוא Router
import { CommonModule } from '@angular/common'; // 2. חובה עבור קומפוננטות standalone

@Component({
  selector: 'app-main',
  standalone: true, // 3. הגדרה כ-standalone
  imports: [CommonModule], // 4. ייבוא CommonModule
  templateUrl: './main.html',
  styleUrl: './main.css',
})
export class Main {
  // 5. הזרקת ה-Router
  constructor(private router: Router) {}

  /**
   * ניווט לטופס הצהרת בריאות (הכפתור הראשון)
   */
  onButton1Click(): void {
    // ⭐️ ניתוב לנתיב טופס הצהרת הבריאות
    this.router.navigate(['/health-declaration']);
  }

  /**
   * ניווט לטופס התשלום שיצרנו (הכפתור השני)
   */
  onButton2Click(): void {
    // ⭐️ ניתוב לנתיב טופס התשלום
    this.router.navigate(['/payment-form']);
  }
}