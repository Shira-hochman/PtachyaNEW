import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { ChildDataService } from '../../services/child-data.service';
import { ChildFilesModalComponent } from '../ChildFilesModalComponent/child-files-modal.component';
import { Child } from '../../../../models/child';

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
    TooltipModule
  ],
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
  kindergartens: any[] = [
    { id: 1, name: 'גן אלון' },
    { id: 2, name: 'גן ברוש' }
  ];

  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  totalPages: number = 0;

  selectedChildForDocs: Child | null = null;
  isDocsModalOpen: boolean = false;

  constructor(private childDataService: ChildDataService) { }

  ngOnInit() {
    this.loadChildren();
  }

  // פונקציית טעינה אחת ויחידה שתומכת בכל הפרמטרים
  loadChildren(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.childDataService.getChildrenPaged(
      this.currentPage, 
      this.pageSize, 
      this.searchTerm, 
      this.selectedKindergartenId
    ).subscribe({
      next: (res: any) => {
        this.children = res.items;
        this.totalItems = res.totalCount;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize);
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading children:', err);
        this.errorMessage = 'שגיאה בטעינת הנתונים מהשרת.';
        this.isLoading = false;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1; // תמיד חוזרים לעמוד הראשון בחיפוש חדש
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

  editChild(childId: number): void {
    console.log('Edit child:', childId);
  }

 openDocsModal(child: Child): void {
    this.selectedChildForDocs = child;
    this.isDocsModalOpen = true;
    this.isAttachmentsMode = false; // מצב טפסים רגיל
}

// הפונקציה שהייתה חסרה וגרמה לשגיאה:
openAttachmentsModal(child: Child): void {
    this.selectedChildForDocs = child;
    this.isDocsModalOpen = true;
    this.isAttachmentsMode = true; // מצב נספחים
}

closeDocsModal(): void {
    this.isDocsModalOpen = false;
    this.selectedChildForDocs = null;
    this.isAttachmentsMode = false;
}
}
