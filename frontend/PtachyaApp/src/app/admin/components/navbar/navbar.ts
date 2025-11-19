// navbar.component.ts (Standalone)
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="admin-navbar">
        <div class="logo">
            <span class="logo-text">מערכת ניהול</span>
        </div>
        <div class="user-info">
            <span>שלום, **{{ managerName }}**</span>
            <button class="logout-btn">🚪 יציאה</button>
        </div>
    </header>
  `,
  styles: [`
    .admin-navbar {
        background-color: #3f51b5; /* כחול ראשי */
        color: white;
        padding: 10px 20px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }
    .logo-text {
        font-size: 1.5rem;
        font-weight: bold;
    }
    .user-info {
        display: flex;
        align-items: center;
        gap: 15px;
    }
    .logout-btn {
        background-color: #f44336; 
        color: white;
        border: none;
        padding: 8px 15px;
        border-radius: 4px;
        cursor: pointer;
    }
  `]
})
export class NavbarComponent {
  @Input() managerName: string = '';
  
  // ניתן להוסיף כאן לוגיקת יציאה מהמערכת
}