// admin-layout.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterOutlet } from '@angular/router'; // ⬅️ קריטי להצגת התוכן הדינמי

// 💡 נניח ש-SidebarComponent נוצר כרכיב עצמאי
// 💡 ורכיב Header/Navbar אם נרצה להוסיף (למשל כפתור יציאה)
// לצורך הפשטות נניח שכללנו אותו כרכיב Standalone
import { SidebarComponent } from '../sidebar/sidebar'; 
import { NavbarComponent } from '../navbar/navbar'; 

@Component({
  selector: 'app-admin-layout',
  standalone: true, 
  imports: [CommonModule, RouterOutlet, SidebarComponent, NavbarComponent], // ⬅️ ייבוא הרכיבים
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css' // נשתמש בקובץ CSS
})
export class AdminLayoutComponent {
  // נתון לדוגמה שאפשר להציג ב-Navbar
 
}