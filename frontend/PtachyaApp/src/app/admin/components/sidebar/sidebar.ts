import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent {
  
  menuItems = [
    { label: 'לוח מחוונים', icon: 'home', link: '/admin/dashboard' },
    { label: 'ניהול ילדים', icon: 'users', link: '/admin/children' },
    { label: 'ניהול גנים', icon: 'school', link: '/admin/gardens' },
    { label: 'ניהול טפסים', icon: 'file-text', link: '/admin/forms' },
    { label: 'ניהול תשלומים', icon: 'dollar-sign', link: '/admin/payments' },
    { label: 'עדכון נתונים (Excel)', icon: 'upload-cloud', link: '/admin/update-data' }
  ];
}