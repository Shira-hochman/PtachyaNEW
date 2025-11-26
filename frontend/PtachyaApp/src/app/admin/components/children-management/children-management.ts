import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { HttpClientModule } from '@angular/common/http'; 
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ChildDataService, ChildDto } from '../../services/child-data.service';
// import { EditChildModalComponent } from '../edit-child-modal/edit-child-modal.component'; // ⬅️ ייבוא המודל לעריכה

@Component({
  selector: 'app-children-management', 
  standalone: true, 
  imports: [CommonModule, HttpClientModule, FormsModule, RouterLink], // ⬅️ הוספת המודל
  templateUrl: './children-management.html', 
  styleUrl: './children-management.css'
})
export class ChildrenManagementComponent implements OnInit {
  
  children: ChildDto[] | null = null; 
  isLoading: boolean = false;
  errorMessage: string | null = null;
  
  // פילטרים
  searchTerm: string = '';
  selectedKindergartenId: number | null = null;
  kindergartens: any[] = [{ id: 1, name: 'גן אלון' }, { id: 2, name: 'גן ברוש' }]; 
  
  // ⭐️ משתנים לניהול המודל
  isModalOpen: boolean = false;
  childToEdit: ChildDto | null = null; 

  constructor(private childDataService: ChildDataService) { }

  ngOnInit() {
    this.loadChildren(); 
  }

  /**
   * טוען ומסנן את רשימת הילדים (צד לקוח לצורך הדגמה)
   */
  loadChildren(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.children = null; 
    
    this.childDataService.getAllChildren().subscribe({
        next: (data: ChildDto[]) => {
            let filteredData = data;
            
            // סינון לפי טקסט
            if (this.searchTerm) {
                const term = this.searchTerm.toLowerCase();
                filteredData = filteredData.filter(child => 
                    child.firstName.toLowerCase().includes(term) ||
                    child.lastName.toLowerCase().includes(term) ||
                    child.idNumber.includes(term)
                );
            }
            // סינון לפי גן (מניעת שגיאת סוג)
            if (this.selectedKindergartenId !== null) {
                const selectedId = Number(this.selectedKindergartenId);
                filteredData = filteredData.filter(child => Number(child.kindergartenId) === selectedId);
            }
            
            this.children = filteredData;
            this.isLoading = false;
            this.errorMessage = filteredData.length === 0 ? 'לא נמצאו ילדים תואמים לחיפוש.' : null;
        },
        error: (err: any) => {
            console.error('Failed to load children:', err);
            this.errorMessage = 'שגיאה בטעינת הנתונים.';
            this.isLoading = false;
        }
    });
  }

  /**
   * פותח את חלון עריכת הילד הנבחר
   */
  editChild(childId: number): void {
    const selectedChild = this.children?.find(c => c.childId === childId);
    if (selectedChild) {
      this.childToEdit = selectedChild;
      this.isModalOpen = true; // פתיחת המודל
    }
  }

  /**
   * מטפל באירוע שמירת הילד מהמודל
   */
  handleChildUpdate(updatedChild: ChildDto): void {
    // עדכון רשימת הילדים הנוכחית ב-UI (לצורך רענון מיידי)
    if (this.children) {
      const index = this.children.findIndex(c => c.childId === updatedChild.childId);
      if (index > -1) {
        this.children[index] = updatedChild; 
      }
    }
    this.isModalOpen = false; 
    this.childToEdit = null;
  }
  
  /**
   * סוגר את המודל (ביטול או שמירה)
   */
  handleModalClose(): void {
    this.isModalOpen = false;
    this.childToEdit = null;
  }
}