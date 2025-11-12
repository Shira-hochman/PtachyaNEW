import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms'; // 1. חובה עבור [(ngModel)]
import { CommonModule } from '@angular/common'; // 2. חובה עבור קומפוננטות standalone
import { Router } from '@angular/router'; 
import { ChildAuthService } from '../../services/child-auth.service'; 
import { Child } from '../../../../models/child'; // ייבוא המודל

@Component({
  selector: 'app-login',
  // 4. הקומפוננטה היא standalone
  standalone: true, 
  imports: [
    FormsModule, 
    CommonModule 
  ],
  templateUrl: './login.html', // 5. ודא ששם הקובץ הוא 'login.html'
  styleUrl: './login.css',
})
export class Login implements OnInit { 
  
  // 6. הוספת המשתנים הנדרשים על ידי ה-HTML
  childId: string = ''; 
  birthDate: string = '';
  message: string = '';

  // הודעות השגיאה הצפויות מהשרת (כדי להבדיל הצלחה מכישלון)
  private errorMessages = ['שגוי', 'אחד מהנתונים שהוקש שגוי'];

  // 7. הזרקת ה-Service וה-Router
  constructor(
    private authService: ChildAuthService, 
    private router: Router
  ) {}
  
  ngOnInit(): void {
  }

 login() {
    this.message = 'בדיקת נתונים...';
    
    if (!this.childId || !this.birthDate) {
        this.message = 'יש למלא את כל השדות.';
        return;
    }
    
    this.authService.getChildDetails(this.childId, this.birthDate).subscribe({
      next: (child: Child) => {
        this.message = `התחברות מוצלחת! ברוך הבא, ${child.firstName} ${child.lastName}`;
        this.router.navigate(['/main']); 
      },
      error: (err) => {
        // טיפול בשגיאת רשת או שגיאת סטטוס HTTP
        const errorMsg = err.error?.title || err.error?.message || 'אחד מהנתונים שהוקש שגוי. נסה שנית.';
        this.message = errorMsg; 
        console.error('Login failed:', err);
      }
    });
}
}
