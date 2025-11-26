import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterLink } from '@angular/router'; // ⬅️ ייבוא לניווט מהיר

@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [CommonModule, RouterLink], // הוספנו RouterLink
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  
  // נתונים שיבואו מהשרת (דוגמא)
  stats = {
    totalChildren: 450,
    activeGardens: 15,
    pendingForms: 7,
    unpaidPayments: 12
  };
  
  isLoading: boolean = true;

  // constructor(private adminService: AdminService) { }

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    // סימולציה של טעינת נתונים
    setTimeout(() => {
        this.isLoading = false;
    }, 1000);
  }
}