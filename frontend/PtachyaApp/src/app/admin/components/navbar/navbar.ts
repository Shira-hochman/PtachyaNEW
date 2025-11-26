import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 
import { LoginService } from '../../services/login'; // ⬅️ 1. ייבוא ה-LoginService

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent implements OnInit {
  
  // 💡 זה יקבל את השם מהלוגין סרוויס
  managerName: string = 'טוען...'; 
  
  // ⬅️ 2. הזרקת LoginService
  constructor(private router: Router, private loginService: LoginService) { } 

  ngOnInit() {
    // ⭐️ 3. קריאה לשליפת שם המשתמש (במקום הסימולציה)
    const username = this.loginService.getCurrentUsername();
    
    if (username) {
        this.managerName = `שלום, ${username}`;
    } else {
        // אם לא נמצא משתמש מחובר, נציג שם ברירת מחדל
        this.managerName = 'מנהל אורח';
    }
  }

  logout(): void {
    // ⭐️ 4. שימוש ב-LoginService ליציאה
    this.loginService.logout();
  }
}