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
  @Input() showAttachmentsOnly: boolean = false; 
  @Output() close = new EventEmitter<void>();

  files: any[] = []; 
  isLoading: boolean = false;
  errorMessage: string | null = null;
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
    next: (data: ChildFormDto[]) => {
      if (this.showAttachmentsOnly) {
        const allAttachments: any[] = [];
        data.forEach((form: ChildFormDto) => {
          if (form.attachmentUrls && form.attachmentUrls.length > 0) {
            form.attachmentUrls.forEach((url: string, index: number) => {
              allAttachments.push({
                fileName: `נספח ${index + 1} - ${this.translateFormType(form.formType)}`,
                uploadDate: form.uploadDate,
                downloadUrl: url
              });
            });
          }
        });
        this.files = allAttachments;
      } else {
        this.files = data;
      }

      this.isLoading = false;
      // הסרנו מכאן את ה-if שקובע errorMessage
    },
    error: (err: any) => {
      this.errorMessage = 'שגיאה בתקשורת עם השרת.'; // כאן נשאיר שגיאה אמיתית
      this.isLoading = false;
    }
  });
}

  translateFormType(type: string): string {
    const types: { [key: string]: string } = {
      'HEALTH_DECLARATION': 'הצהרת בריאות',
      'DISCOUNT_REQUEST': 'בקשת הנחה'
    };
    return types[type] || type;
  }

  downloadFile(url: string) {
    if (!url) return;
    this.formDataService.downloadFileByUrl(url).subscribe({
      next: (blob: Blob) => {
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
    setTimeout(() => {
      this.close.emit();
    }, 100);
  }
}