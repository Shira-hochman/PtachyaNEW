import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { RippleModule } from 'primeng/ripple'; // להוספת אפקט לחיצה

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RippleModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent {
  
  menuItems = [
    { label: 'לוח מחוונים', icon: 'pi pi-home', link: '/admin/dashboard' },
    { label: 'ניהול ילדים', icon: 'pi pi-users', link: '/admin/children' },
    { label: 'ניהול גנים', icon: 'pi pi-building', link: '/admin/kindergartens/manage' },
    { label: 'ניהול טפסים', icon: 'pi pi-file-edit', link: '/admin/forms' },
    { label: 'ניהול תשלומים', icon: 'pi pi-credit-card', link: '/admin/payments' },
    { label: 'עדכון נתונים', icon: 'pi pi-cloud-upload', link: '/admin/update-data' }
  ];
}