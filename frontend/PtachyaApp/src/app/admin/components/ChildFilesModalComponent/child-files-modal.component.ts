import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { FormDataService, ChildFormDto } from '../../services/form-data.service';

@Component({
  selector: 'app-child-files-modal',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    TableModule,
    ButtonModule,
    ProgressSpinnerModule,
    MessageModule
  ],
  templateUrl: './child-files-modal.component.html',
  styleUrls: ['./child-files-modal.component.css']
})
export class ChildFilesModalComponent implements OnInit {
  @Input() childIdNumber!: string;
  @Input() childName!: string;
  @Output() close = new EventEmitter<void>();

  files: ChildFormDto[] = [];
  isLoading: boolean = false;
  errorMessage: string | null = null;
  
  // הגדרה ל-true מיד, כדי למנוע מצב שהדיאלוג נשאר סגור
  isVisible: boolean = true; 

  constructor(private formDataService: FormDataService) {}

  ngOnInit(): void {
    if (this.childIdNumber) {
      this.loadFiles();
    }
  }

  loadFiles() {
    this.isLoading = true;
    this.files = [];
    this.errorMessage = null;

    this.formDataService.getFormsByIdNumber(this.childIdNumber).subscribe({
      next: (data) => {
        this.files = data;
        this.isLoading = false;
        if (!this.files || this.files.length === 0) {
          this.errorMessage = 'לא נמצאו טפסים עבור הילד.';
        }
      },
      error: (err) => {
        console.error('Error fetching files:', err);
        this.errorMessage = 'שגיאה בטעינת הטפסים.';
        this.isLoading = false;
      }
    });
  }

  downloadFile(url: string) {
    if (!url) return;
    this.formDataService.downloadFileByUrl(url).subscribe({
      next: (blob) => {
        const fileUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = fileUrl;
        const fileNameMatch = url.match(/fileName=([^&]+)/i);
        link.download = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : 'document.pdf';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(fileUrl);
      }
    });
  }

  closeModal() {
    this.isVisible = false;
    // נתינת זמן לאנימציית הסגירה לפני השמדת הקומפוננטה
    setTimeout(() => {
      this.close.emit();
    }, 100);
  }
}