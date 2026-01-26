import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
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
  // preview state
  previewVisible: boolean = false;
  previewSrc: SafeResourceUrl | null = null; // sanitized URL for iframe/img
  previewObjectUrl: string | null = null; // raw object URL for openInNewTab / revoke
  previewFileName: string | null = null;
  previewMime: string | null = null;

  constructor(private formDataService: FormDataService, private sanitizer: DomSanitizer) {}

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

  this.formDataService.downloadFile(url).subscribe(blob => {
    const a = document.createElement('a');
    const objectUrl = URL.createObjectURL(blob);
    a.href = objectUrl;
    a.download = '';
    a.click();
    URL.revokeObjectURL(objectUrl);
  });
}

  viewFile(url: string, fileName?: string) {
    if (!url) return;
    this.formDataService.downloadFile(url).subscribe(blob => {
      const objectUrl = URL.createObjectURL(blob);
      this.previewObjectUrl = objectUrl;
      this.previewSrc = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
      this.previewFileName = fileName || 'Preview';
      this.previewMime = blob.type || null;
      this.previewVisible = true;
    }, err => {
      this.errorMessage = 'שגיאה בטעינת הקובץ לתצוגה.';
    });
  }

  openInNewTab() {
    if (!this.previewObjectUrl) return;
    window.open(this.previewObjectUrl, '_blank');
  }

  closePreview() {
    this.previewVisible = false;
    if (this.previewObjectUrl) {
      try { URL.revokeObjectURL(this.previewObjectUrl); } catch {}
      this.previewObjectUrl = null;
    }
    this.previewSrc = null;
    this.previewFileName = null;
    this.previewMime = null;
  }




  closeModal() {
    // ensure preview objectURLs are cleaned
    this.closePreview();
    this.isVisible = false;
    setTimeout(() => {
      this.close.emit();
    }, 100);
  }
}