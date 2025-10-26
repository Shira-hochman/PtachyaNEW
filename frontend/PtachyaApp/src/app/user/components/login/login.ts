// src/app/user/components/login/login.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // ⬅️ חובה עבור [(ngModel)]
import { LoginService } from '../../service/login'; // ✅ נתיב עדכני

@Component({
  selector: 'app-login',
  standalone: true, // ⬅️ הפוך לרכיב עצמאי
  imports: [FormsModule], // ⬅️ ייבוא FormsModule
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  username = '';
  password = '';
  message = '';

  // השם המקובל לקובץ הוא login.service.ts
  constructor(private loginService: LoginService) {} 

  login() {
    this.loginService.login(this.username, this.password).subscribe({
      next: (res) => {
        // אם הצלחה - הצגת שם המשתמש
        if (res.isSuccess) {
           this.message = `ברוך הבא, ${this.username}! `; // ⬅️ שינוי לפי בקשתך
        } else {
           this.message = res.message || 'שגיאה בהתחברות';
        }
      },
      error: (err) => {
        // אם כשלון - הצגת הודעת שגיאה מותאמת
        this.message = 'אין שם משתמש כזה. פנה למנהל המערכת.'; // ⬅️ שינוי לפי בקשתך
      }
    });
  }
}