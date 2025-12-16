import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { HttpClientModule } from '@angular/common/http'; 
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ChildDataService } from '../../services/child-data.service'; 

// ⭐️ ייבוא המודל החדש שיצרנו (ודאי שהנתיב תואם למבנה התיקיות שלך)
import { ChildFilesModalComponent } from '../ChildFilesModalComponent/child-files-modal.component';

// הגדרת המבנה
export interface ChildUi {
  childId: number;
  firstName: string;
  lastName: string;
  idNumber: string; // שדה חובה למודל הקבצים
  birthDate: Date;
  kindergartenId: string;
  email: string;
  formLink: string;
  paymentId?: number;
}

@Component({
  selector: 'app-children-management', 
  standalone: true, 
  // ⭐️ הוספנו את ChildFilesModalComponent לרשימת ה-imports
  imports: [CommonModule, HttpClientModule, FormsModule, RouterLink, ChildFilesModalComponent],
  templateUrl: './children-management.html', 
  styleUrl: './children-management.css'
})
export class ChildrenManagementComponent implements OnInit {
  
  children: ChildUi[] | null = null; 
  isLoading: boolean = false;
  errorMessage: string | null = null;
  
  // פילטרים
  searchTerm: string = '';
  selectedKindergartenId: number | null = null;
  kindergartens: any[] = [{ id: 1, name: 'גן אלון' }, { id: 2, name: 'גן ברוש' }]; 

  // משתנים לפייג'ינג
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  totalPages: number = 0;

  // ⭐️ משתנים חדשים לניהול המודל (Pop-up)
  selectedChildForDocs: ChildUi | null = null; // הילד שנבחר להצגת מסמכים
  isDocsModalOpen: boolean = false;            // האם המודל פתוח?

  constructor(private childDataService: ChildDataService) { }

  ngOnInit() {
    this.loadChildren(); 
  }

  loadChildren(): void {
    this.isLoading = true;
    this.errorMessage = null;
    
    this.childDataService.getChildrenPaged(this.currentPage, this.pageSize).subscribe({
      next: (res: any) => {
        this.children = res.items; 
        
        this.totalItems = res.totalCount; 
        this.totalPages = Math.ceil(this.totalItems / this.pageSize); 
        this.isLoading = false;
        
        if (!this.children || this.children.length === 0) {
             this.errorMessage = 'לא נמצאו נתונים.';
        }
      },
      error: (err: any) => {
        console.error('Error loading children:', err);
        this.errorMessage = 'שגיאה בטעינת הנתונים.';
        this.isLoading = false;
      }
    });
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadChildren();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadChildren();
    }
  }

  editChild(childId: number): void {
    console.log('Edit child:', childId);
    // לוגיקת פתיחת מודל עריכה... (אם תרצי להוסיף בעתיד)
  }

  // ⭐️ פונקציה לפתיחת מודל המסמכים
  openDocsModal(child: ChildUi): void {
    this.selectedChildForDocs = child;
    this.isDocsModalOpen = true;
  }

  // ⭐️ פונקציה לסגירת מודל המסמכים
  closeDocsModal(): void {
    this.isDocsModalOpen = false;
    this.selectedChildForDocs = null;
  }
}