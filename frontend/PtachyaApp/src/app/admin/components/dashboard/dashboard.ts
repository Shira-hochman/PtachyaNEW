import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterLink } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';

// PrimeNG 20 Modules
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';

import { environment } from '../../../../environments/environment';
@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [
    CommonModule, 
    RouterLink, 
    HttpClientModule, // וודאי שזה כאן אם את מבצעת קריאות HTTP
    CardModule, 
    ButtonModule, 
    SkeletonModule, 
    RippleModule,
    TooltipModule
  ], 
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
  private apiUrl = `${environment.apiBaseUrl}/api/Dashboard/stats`;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    this.isLoading = true;
    this.http.get<any>(this.apiUrl).subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load stats', err);
        this.isLoading = false;
      }
    });
  }
}