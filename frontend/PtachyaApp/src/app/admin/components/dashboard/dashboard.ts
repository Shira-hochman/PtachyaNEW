import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http'; // חובה לייבא

@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [CommonModule, RouterLink], 
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  
  stats = {
    totalChildren: 0,
    activeGardens: 0,
    pendingForms: 0,
    unpaidPayments: 0
  };
  
  isLoading: boolean = true;
  private apiUrl = 'https://localhost:7222/api/Dashboard/stats'; // הכתובת שיצרנו

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    this.http.get<any>(this.apiUrl).subscribe({
      next: (data) => {
        this.stats = data; // הנתונים האמיתיים מהשרת!
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load stats', err);
        this.isLoading = false;
      }
    });
  }
}