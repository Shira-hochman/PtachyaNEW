import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

// שירותים ודגמים
import { GardenDataService, KindergartenDto } from '../../services/garden-data.service';

// רכיבי PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { RippleModule } from 'primeng/ripple';

@Component({
  selector: 'app-kindergarten-management',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    HttpClientModule, 
    FormsModule,
    TableModule,
    ButtonModule,
    ToolbarModule,
    InputTextModule,
    ProgressSpinnerModule,
    TooltipModule,
    RippleModule
  ],
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

    this.kgService.getAllGardens().subscribe({
      next: (data) => {
        this.kindergartens = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading kindergartens:', err);
        this.errorMessage = 'שגיאה בטעינת נתוני הגנים. וודא שיש לך הרשאות מתאימות.'; 
        this.isLoading = false;
      }
    });
  }

  deleteKindergarten(id: number): void {
    if (confirm(`האם אתה בטוח שברצונך למחוק את גן מספר ${id}?`)) {
      this.kgService.deleteGarden(id).subscribe({
        next: () => {
          if (this.kindergartens) {
            this.kindergartens = this.kindergartens.filter(k => k.kindergartenId !== id);
          }
          alert('הגן נמחק בהצלחה מהמערכת.');
        },
        error: (err) => {
          console.error('Error deleting kindergarten:', err);
          alert('פעולת המחיקה נכשלה. ייתכן והגן מקושר לנתונים אחרים.');
        }
      });
    }
  }
}