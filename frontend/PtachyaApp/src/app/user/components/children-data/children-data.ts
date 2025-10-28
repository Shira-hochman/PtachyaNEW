import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { HttpClientModule } from '@angular/common/http'; 
import { ChildDataService, ChildDto } from '../../services/child-data.service'; // ⬅️ ייבוא ה-Service וה-Dto
import { Observable } from 'rxjs';

@Component({
  selector: 'app-children-data',
  standalone: true, 
  imports: [CommonModule, HttpClientModule], 
  templateUrl: './children-data.html',
  styleUrl: './children-data.css'
})
export class ChildrenData implements OnInit {
  
  children: ChildDto[] | null = null; // המשתנה שיחזיק את רשימת הילדים
  isLoading: boolean = false;
  errorMessage: string | null = null;

  // ⬅️ הזרקת ChildDataService
  constructor(private childDataService: ChildDataService) { }

  ngOnInit() {
    // ניתן להשאיר ריק או לטעון אוטומטית: this.loadChildren();
  }

  /**
   * טוען את כל רשימת הילדים מהשרת
   */
  loadChildren(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.children = null; 

    this.childDataService.getAllChildren().subscribe({
      next: (data: ChildDto[]) => {
        this.children = data;
        this.isLoading = false;
        this.errorMessage = data.length === 0 ? 'לא נמצאו ילדים במערכת.' : null;
      },
      error: (err: any) => {
        console.error('Failed to load children:', err);
        this.errorMessage = 'שגיאה בטעינת הנתונים: אנא נסה שוב מאוחר יותר.';
        this.isLoading = false;
      }
    });
  }
}