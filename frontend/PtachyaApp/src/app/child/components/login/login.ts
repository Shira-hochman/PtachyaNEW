import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms'; // 1. חובה עבור [(ngModel)]
import { CommonModule } from '@angular/common'; // 2. חובה עבור קומפוננטות standalone
import { Router } from '@angular/router'; 
import { ChildAuthService } from '../../services/child-auth.service'; 


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
    // ניתן להוסיף לוגיקה שתרוץ בעת טעינת הקומפוננטה
  }

  // 8. הוספת הפונקציה login() כפי שהיא מופעלת ב-HTML
  login() {
    this.message = 'בדיקת נתונים...';
    
    if (!this.childId || !this.birthDate) {
        this.message = 'יש למלא את כל השדות.';
        return;
    }

    // קריאה לשירות האימות
    this.authService.verifyIdentity(this.childId, this.birthDate).subscribe({
      next: (res: string) => {
        
        // 🛑 התיקון שהוספת: מנקים גרשיים מיותרים משני הצדדים (אם קיימים)
        const cleanRes = res.replace(/^"|"$/g, ''); 
        
        // בדיקת הצלחה: אם התשובה היא לא אחת מהודעות השגיאה
        if (!this.errorMessages.includes(cleanRes)) { 
          const fullName = cleanRes; // התשובה היא השם המלא של הילד
          this.message = `התחברות מוצלחת! ברוך הבא, ${fullName}`;
          
          // ⚠️ ניתוב לעמוד הבא (data-update)
          this.router.navigate(['/main', fullName]); 
          
        } else {
          // כישלון: הצגת הודעת השגיאה
          this.message = cleanRes; 
        }
      },
      error: (err) => {
        // טיפול בשגיאת רשת או שגיאת סטטוס HTTP
        if (err.status === 400) {
          // מנסים לקבל הודעת שגיאה מגוף התשובה של ה-BadRequest
          this.message = err.error?.title || 'שגיאה בנתונים שהוזנו.'; 
        } else {
          this.message = 'שגיאה בחיבור לשרת.';
        }
        console.error('Login failed:', err);
      }
    });
  }
}
