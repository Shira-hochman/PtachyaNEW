// sidebar.component.ts (Standalone)
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router'; // ⬅️ לניווט

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="admin-sidebar">
        <div class="menu-item" routerLink="/admin/dashboard" routerLinkActive="active-link">
            🏠 לוח מחוונים
        </div>
        <div class="menu-item" routerLink="/admin/children" routerLinkActive="active-link">
            👧 ניהול ילדים
        </div>
        <div class="menu-item" routerLink="/admin/gardens" routerLinkActive="active-link">
            🌳 ניהול גנים
        </div>
        <div class="menu-item" routerLink="/admin/forms" routerLinkActive="active-link">
            📄 ניהול טפסים
        </div>
        <div class="menu-item" routerLink="/admin/payments" routerLinkActive="active-link">
            💰 ניהול תשלומים
        </div>
        <div class="menu-item" routerLink="/admin/update-data" routerLinkActive="active-link">
            📤 עדכון נתונים (Excel)
        </div>
    </nav>
  `,
  styles: [`
    .admin-sidebar {
        width: 250px;
        min-width: 200px; /* רוחב קבוע */
        background-color: #ffffff; 
        box-shadow: 2px 0 5px rgba(0, 0, 0, 0.05);
        padding-top: 20px;
    }
    .menu-item {
        padding: 15px 20px;
        margin: 5px 0;
        cursor: pointer;
        color: #333;
        font-weight: 500;
        transition: background-color 0.2s, color 0.2s;
        border-right: 4px solid transparent; /* למראה נקי */
    }
    .menu-item:hover {
        background-color: #e8eaf6; 
        color: #3f51b5; 
    }
    .active-link {
        background-color: #c5cae9; /* צבע רקע לפריט הפעיל */
        color: #3f51b5; /* צבע טקסט לפריט הפעיל */
        border-right-color: #3f51b5; /* סמן ויזואלי חזק */
        font-weight: bold;
    }
  `]
})
export class SidebarComponent {}