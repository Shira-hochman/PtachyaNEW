import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// ⭐️ שירותים
import { ChildDataService } from '../../services/child-data.service';
import { FormDataService } from '../../services/form-data.service';

import { ChildFilesModalComponent } from '../ChildFilesModalComponent/child-files-modal.component';
import { Child } from '../../../../models/child';

// PrimeNG Imports
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { BadgeModule } from 'primeng/badge';
import { ToolbarModule } from 'primeng/toolbar';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-children-management',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    RouterModule,
    ChildFilesModalComponent,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    CardModule,
    MessageModule,
    BadgeModule,
    ToolbarModule,
    SelectModule,
    TooltipModule,
    ConfirmDialogModule,
    DialogModule
  ],
  providers: [ConfirmationService],
  templateUrl: './children-management.html',
  styleUrls: ['./children-management.css']
})
export class ChildrenManagementComponent implements OnInit {
  children: Child[] = [];
  isLoading: boolean = false;
  errorMessage: string | null = null;
  isAttachmentsMode: boolean = false;
  searchTerm: string = '';
  selectedKindergartenId: number | null = null;

  // המערך יתמלא מהשרת
  kindergartens: any[] = [];

  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  totalPages: number = 0;

  selectedChildForDocs: Child | null = null;
  isDocsModalOpen: boolean = false;

  isEditModalOpen: boolean = false;
  childToEdit: Child = {} as Child;

  currentHebrewYear: string = '';
selectedYear: string = '';
yearOptions: string[] = [];
  constructor(
    private childDataService: ChildDataService,
    private formService: FormDataService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit() {
 this.calculateHebrewYears(); // חישוב השנים להצגה
  this.loadKindergartens();
  this.loadChildren();
  }

  loadKindergartens(): void {
    this.childDataService.getAllKindergartens().subscribe({
      next: (data) => {
        this.kindergartens = data;
      },
      error: (err) => {
        console.error('Failed to load kindergartens', err);
      }
    });
  }

  calculateHebrewYears() {
  // לוגיקה פשוטה לקביעת השנה הנוכחית לפי תאריך לועזי
  // (ניתן לשכלל עם ספריה, אבל לצורך התצוגה נחשב לפי ספטמבר/ראש השנה)
  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth() + 1;
  
  // חישוב בסיסי: אם עברנו את ספטמבר, אנחנו כבר בשנה העברית הבאה
  let baseYear = year + 3760;
  if (month >= 9) baseYear++; 
  
  // פונקציה להמרת מספר לשנה עברית (למשל 5786 -> תשפ"ו)
  const toHebrew = (y: number) => {
    const years: { [key: number]: string } = {
      5784: 'תשפ"ד', 5785: 'תשפ"ה', 5786: 'תשפ"ו', 5787: 'תשפ"ז', 5788: 'תשפ"ח'
    };
    return years[y] || y.toString();
  };

  this.currentHebrewYear = toHebrew(baseYear);
  this.selectedYear = this.currentHebrewYear; // ברירת מחדל
  
  // יצירת 3 השנים לכפתורים
  this.yearOptions = [
    toHebrew(baseYear),     // נוכחית
    toHebrew(baseYear - 1), // שנה אחורה
    toHebrew(baseYear - 2)  // שנתיים אחורה
  ];
}

// עדכון פונקציית הטעינה
loadChildren(): void {
  this.isLoading = true;
  this.childDataService.getChildrenPaged(
    this.currentPage,
    this.pageSize,
    this.searchTerm,
    this.selectedKindergartenId,
    this.selectedYear // <-- שליחת השנה המסוננת
  ).subscribe({
    next: (res: any) => {
      this.children = res.items;
      this.totalItems = res.totalCount;
      this.totalPages = Math.ceil(this.totalItems / this.pageSize);
      this.isLoading = false;
    },
    error: () => this.isLoading = false
  });
}

selectYear(year: string) {
  this.selectedYear = year;
  this.currentPage = 1;
  this.loadChildren();
}
  approveChildPayment(child: Child, event: Event): void {
    event.stopPropagation();

    this.confirmationService.confirm({
      message: `האם לאשר את הסדר התשלום עבור ${child.firstName} ${child.lastName}? פעולה זו תאשר את בקשת ההנחה הממתינה.`,
      header: 'אישור תשלום',
      icon: 'pi pi-check-circle',
      acceptLabel: 'אשר תשלום',
      rejectLabel: 'ביטול',
      accept: () => {
        this.formService.getFormsByIdNumber(child.idNumber).subscribe({
          next: (forms) => {
            const pendingForm = forms.find(f => f.status === 'Pending' && f.formType === 'DISCOUNT_REQUEST');

            if (pendingForm) {
              this.formService.approveForm(pendingForm.formId).subscribe({
                next: () => {

                  this.loadChildren();
                  alert('הטופס אושר בהצלחה!');

                },
                error: () => alert('שגיאה באישור הטופס')
              });
            } else {
              alert('לא נמצא טופס בקשת הנחה שממתין לאישור עבור ילד זה.');
            }
          },
          error: (err) => console.error(err)
        });
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadChildren();
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

  openDocsModal(child: Child): void {
    this.selectedChildForDocs = child;
    this.isDocsModalOpen = true;
    this.isAttachmentsMode = false;
  }

  openAttachmentsModal(child: Child): void {
    this.selectedChildForDocs = child;
    this.isDocsModalOpen = true;
    this.isAttachmentsMode = true;
  }

  closeDocsModal(): void {
    this.isDocsModalOpen = false;
    this.selectedChildForDocs = null;
    this.isAttachmentsMode = false;
  }

  editChild(childId: number): void {
    const originalChild = this.children.find(c => c.childId === childId);

    if (originalChild) {
      this.childToEdit = { ...originalChild };
      this.isEditModalOpen = true;
    }
  }

  saveChildChanges(): void {
    this.isLoading = true;

    // שימוש ב-as any לעקיפת חוסר התאמות קטנות בטיפוסים
    this.childDataService.updateChild(this.childToEdit as any).subscribe({
      next: () => {
        const index = this.children.findIndex(c => c.childId === this.childToEdit.childId);
        if (index !== -1) {
          this.children[index] = { ...this.childToEdit };

          const selectedGarden = this.kindergartens.find(k => k.id === this.childToEdit.kindergartenId);
          if (selectedGarden) {
            (this.children[index] as any).kindergartenName = selectedGarden.name;
          }

          this.children = [...this.children];
        }

        this.isEditModalOpen = false;
        this.isLoading = false;
        alert('פרטי הילד עודכנו בהצלחה!');
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert('שגיאה בעדכון הפרטים');
      }
    });
  }

  cancelEdit(): void {
    this.isEditModalOpen = false;
  }
}