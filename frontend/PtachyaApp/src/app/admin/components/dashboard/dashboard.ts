// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
// נניח שייצרתם service לנתוני מנהל (AdminService)
// import { AdminService } from '../../services/admin.service';

@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [CommonModule], 
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  
  // נתונים שיבואו מהשרת (דוגמא)
  stats = {
    totalChildren: 0,
    activeGardens: 0,
    pendingForms: 0,
    unpaidPayments: 0
  };
  
  isLoading: boolean = true;

  // constructor(private adminService: AdminService) { }

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    // 💡 כאן נבצע קריאת API שתחזיר את כל המידע הסטטיסטי
    
    // סימולציה של טעינת נתונים
    setTimeout(() => {
        this.stats = {
            totalChildren: 450,
            activeGardens: 15,
            pendingForms: 7,
            unpaidPayments: 12
        };
        this.isLoading = false;
    }, 1000);
  }
}