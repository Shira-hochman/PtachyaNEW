import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router'; // הוספנו RouterOutlet
import { CommonModule } from '@angular/common'; // מומלץ תמיד ב-Standalone

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterOutlet, CommonModule], // חייב להופיע כאן!
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  userName: string = 'הורה יקר';
  currentDate: string = '';

  constructor(private router: Router) {}

  ngOnInit(): void {
    const now = new Date();
    this.currentDate = now.toLocaleDateString('he-IL', { 
      weekday: 'long', 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  }

  logout(): void {
    this.router.navigate(['/login']);
  }
}