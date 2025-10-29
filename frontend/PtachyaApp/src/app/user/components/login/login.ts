import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; 
import { Router } from '@angular/router'; 
import { LoginService } from '../../services/login'; // ✅ תיקון נתיב הייבוא

@Component({
  selector: 'app-login',
  standalone: true, 
  imports: [FormsModule], 
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  username = '';
  password = '';
  message = '';

  constructor(
    private loginService: LoginService,
    private router: Router
  ) {}

  login() {
    this.loginService.login(this.username, this.password).subscribe({
      next: (res) => {
        // אם הצלחה - בצע ניתוב
        if (res.isSuccess) {
          // ✅ תיקון אבטחה: ניתוב למסך הראשי ללא פרמטרים ב-URL
          this.router.navigate(['/data-update']); 
        } else {
          this.message = res.message || 'שגיאה בהתחברות';
        }
      },
      error: (err) => {
        // רצוי לא לציין אם הבעיה היא שם המשתמש או הסיסמה מטעמי אבטחה
        this.message = 'שם משתמש או סיסמה שגויים. נסה שוב.'; 
      }
    });
  }

  // 🚪 מתודות אלה קיימות גם ב-Service, אבל נשאיר אותן כאן לשם הפשטות:
  
  logout() {
    // ⚠️ השתמש ב-Service לביצוע הפעולה
    this.loginService.logout(); 
  }
  
  isLoggedIn(): boolean {
    // ⚠️ השתמש ב-Service לבדיקת הסטטוס
    return this.loginService.isLoggedIn();
  }
}
