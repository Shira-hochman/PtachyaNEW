// src/app/components/kindergarten-management/kindergarten-management.component.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {GardenDataService ,KindergartenDto } from '../../services/garden-data.service';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-kindergarten-management',
  standalone: true,
  imports: [CommonModule, RouterLink, HttpClientModule, FormsModule],
  templateUrl: './kindergarten-management.html', 
  styleUrl: './kindergarten-management.css' 
})
export class KindergartenManagementComponent implements OnInit {
  kindergartens: KindergartenDto[] | null = null;
  isLoading: boolean = false;
  errorMessage: string | null = null;

  constructor(private kgService: GardenDataService) { }

  ngOnInit(): void {
    this.loadKindergartens();
  }

  loadKindergartens(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // ⭐️ תיקון: קורא ל-getAllGardens() במקום getAllKindergartens() ⭐️
    this.kgService.getAllGardens().subscribe({
      next: (data) => {
        this.kindergartens = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading kindergartens:', err);
        // שגיאת 401 תיקלט כאן אם אין הרשאה/טוקן
        this.errorMessage = 'שגיאה בטעינת הגנים. ודא שיש לך הרשאות מנהל.'; 
        this.isLoading = false;
      }
    });
  }

  // ⭐️ פונקציית מחיקה (לוגיקה בסיסית) ⭐️
  deleteKindergarten(id: number): void {
    if (confirm(`האם אתה בטוח שברצונך למחוק את גן ID: ${id}?`)) {
      this.kgService.deleteGarden(id).subscribe({
        next: () => {
          // הסר מהרשימה המקומית ללא טעינה מחדש של כל הנתונים
          this.kindergartens = this.kindergartens!.filter(k => k.kindergartenId !== id);
          alert('הגן נמחק בהצלחה.');
        },
        error: (err) => {
          console.error('Error deleting kindergarten:', err);
          alert('שגיאה במחיקת הגן. ודא שיש לך הרשאות.');
        }
      });
    }
  }
}