import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { FormsModule } from '@angular/forms'; 
import { Router } from '@angular/router'; 
import { LoginService } from '../../services/login'; // ודא שהנתיב נכון

@Component({
  selector: 'app-admin-login',
  standalone: true, 
  imports: [CommonModule, FormsModule], 
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  username = '';
  password = '';
  message = '';
  isLoading = false; // ⭐️ הוספה: משתנה לניהול מצב טעינה (עבור הספינר)

  constructor(
    private loginService: LoginService,
    private router: Router
  ) {}

  login() {
    // אימות בסיסי לפני שליחה
    if (!this.username || !this.password) {
        this.message = 'אנא מלא את כל השדות';
        return;
    }

    this.isLoading = true; // הפעלת ספינר
    this.message = 'מתחבר למערכת...';

    this.loginService.login(this.username, this.password).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.message = 'התחברות מוצלחת! מעביר...';
          // השהייה קצרה כדי שהמשתמש יראה את ההודעה הירוקה
          setTimeout(() => {
              this.router.navigate(['/admin/dashboard']); 
          }, 500);
        } else {
          this.message = res.message || 'שגיאה בהתחברות';
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error(err);
        this.message = 'שם משתמש או סיסמה שגויים.';
        this.isLoading = false;
      }
    });
  }
}