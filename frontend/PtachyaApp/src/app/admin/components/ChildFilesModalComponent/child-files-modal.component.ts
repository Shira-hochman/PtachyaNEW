// src/app/.../child-files-modal.component.ts

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormDataService, ChildFormDto } from '../../services/form-data.service'; // ודאי נתיב

@Component({
  selector: 'app-child-files-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './child-files-modal.component.html',
  styleUrls: ['./child-files-modal.component.css']
})
export class ChildFilesModalComponent implements OnInit {
  @Input() childIdNumber!: string; // מקבל את הת"ז מהאבא
  @Input() childName!: string;     // מקבל את השם לתצוגה
  @Output() close = new EventEmitter<void>(); // משדר לאבא לסגור את המודל

  files: ChildFormDto[] = [];
  isLoading: boolean = false;
  errorMessage: string | null = null;

  constructor(private formDataService: FormDataService) {}

  ngOnInit(): void {
    if (this.childIdNumber) {
      this.loadFiles();
    }
  }

  loadFiles() {
    this.isLoading = true;
    this.formDataService.getFormsByIdNumber(this.childIdNumber).subscribe({
      next: (data) => {
        this.files = data;
        this.isLoading = false;
        this.errorMessage = null; // ניקוי שגיאות ישנות
      },
      error: (err) => {
        console.error('Error fetching files:', err);
        this.errorMessage = 'שגיאה בטעינת הקבצים. נסה שוב מאוחר יותר.';
        this.isLoading = false;
      }
    });
  }

  // ⭐️⭐️⭐️ פונקציית הורדה מעודכנת להשתמש ב-HttpClient ⭐️⭐️⭐️
  downloadFile(url: string) {
    if (!url) return;
    this.errorMessage = null;
    
    this.formDataService.downloadFileByUrl(url).subscribe({
        next: (blob) => {
            // יצירת לינק זמני מהנתונים הבינאריים
            const fileUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = fileUrl;
            
            // מנסה לחלץ שם קובץ מה-URL לשם ההורדה (למשל: filename=...)
            const fileNameMatch = url.match(/fileName=([^&]+)/i);
            link.download = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : 'document.pdf';

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(fileUrl); // שחרור הזיכרון
        },
        error: (err) => {
            console.error('Error during secure file download:', err);
            // אם יש 401, זה ייתפס כאן.
            this.errorMessage = 'שגיאה בהורדת הקובץ. ודא שאתה מחובר.';
        }
    });
  }

  closeModal() {
    this.close.emit();
  }
}