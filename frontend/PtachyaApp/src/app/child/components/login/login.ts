import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // חובה עבור ngIf, ngClass וכו'
import { FormsModule } from '@angular/forms';   // חובה עבור ngModel
import { Router } from '@angular/router';
import { ChildAuthService } from '../../services/child-auth.service'; // ודאי שהנתיב נכון

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  
  childId: string = '';
  birthDate: string = '';
  message: string = '';
  isLoading: boolean = false; // הוספתי משתנה לניהול מצב טעינה (ספינר)

  constructor(
    private authService: ChildAuthService,
    private router: Router
  ) {}

  ngOnInit(): void {}

  login() {
    this.message = '';
    
    // ולידציה בסיסית
    if (!this.childId || !this.birthDate) {
        this.message = 'יש למלא את כל השדות.';
        return;
    }
    
    this.isLoading = true; // הפעלת אנימציית טעינה
    this.message = 'בודק נתונים...';

    this.authService.getChildDetails(this.childId, this.birthDate).subscribe({
      next: (response) => {
        const child = this.authService.getCurrentChild();
        
        if (child) {
           this.message = `התחברות מוצלחת! ברוך הבא, ${child.firstName} ${child.lastName}`;
           // השהייה קצרה כדי שהמשתמש יראה את ההודעה לפני המעבר
           setTimeout(() => {
             this.router.navigate(['/child/main']); 
           }, 1000);
        } else {
             this.isLoading = false;
             this.message = 'התחברות הצליחה אך פרטי הילד לא נשמרו. אנא נסה שנית.';
        }
      },
      error: (err) => {
        this.isLoading = false;
        // חילוץ הודעת השגיאה
        const errorMsg = err.error?.title || err.error?.message || 'אחד מהנתונים שהוקש שגוי. נסה שנית.';
        this.message = errorMsg; 
        console.error('Login failed:', err);
      }
    });
  }
}