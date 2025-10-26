// src/app/user/components/login/login.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; 
import { LoginService } from '../../service/login'; 
import { Router } from '@angular/router'; 

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
          // 🚨 התיקון: ניתוב רק למסך הראשי (data-update)
          this.router.navigate(['/data-update', this.username]);
          // ❌ הוסר הניתוב המיותר שדרס את הקודם: this.router.navigate(['/children-data']);
        } else {
          this.message = res.message || 'שגיאה בהתחברות';
        }
      },
      error: (err) => {
        this.message = 'שם משתמש או סיסמה שגויים'; 
      }
    });
  }
}