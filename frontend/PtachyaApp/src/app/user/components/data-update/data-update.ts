// data-update.ts
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router'; // 🚨 RouterLink מגיע מכאן
import { CommonModule } from '@angular/common'; 
import { HttpClientModule } from '@angular/common/http'; 
import { Observable } from 'rxjs'; 
import { ExcelUpdateService } from '../../service/excel-update.service'; 

@Component({
  selector: 'app-data-update',
  standalone: true,
  // 🚨 ודא ש-RouterLink כלול ב-imports
  imports: [CommonModule, HttpClientModule, RouterLink], 
  templateUrl: './data-update.html',
  styleUrls: ['./data-update.css']
})
export class DataUpdateComponent implements OnInit {
  username: string | null = '';
  statusMessage: string | null = null;
  uploadType: 'children' | 'gardens' | null = null;

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    private route: ActivatedRoute,
    private excelUpdateService: ExcelUpdateService
  ) {}

  ngOnInit() {
    this.username = this.route.snapshot.paramMap.get('username');
  }

  openFileInput(type: 'children' | 'gardens') {
    this.uploadType = type;
    this.statusMessage = null; 
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];

    if (file) {
      if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) { 
        this.statusMessage = 'שגיאה: ניתן לעלות קבצי Excel (xlsx/xls) בלבד.';
        this.fileInput.nativeElement.value = '';
        return;
      }
      this.uploadFile(file);
    }
  }

  uploadFile(file: File) {
    if (!this.uploadType) {
      this.statusMessage = 'שגיאה פנימית: סוג העדכון אינו מוגדר.';
      return;
    }

    this.statusMessage = `מעלה קובץ לטעינת ${this.uploadType === 'children' ? 'ילדים' : 'גנים'}... אנא המתן.`;
    
    let upload$: Observable<string>; 

    if (this.uploadType === 'children') {
      upload$ = this.excelUpdateService.uploadChildren(file); 
    } else {
      upload$ = this.excelUpdateService.uploadGardens(file); 
    }

    upload$.subscribe({
      next: (res: string) => { 
        this.statusMessage = `העלאה ועיבוד הצליחו! ${res}`;
      },
      error: (err: any) => { 
        console.error('שגיאת שרת:', err);
        
        const serverMessage = err.error && typeof err.error === 'string' 
                            ? err.error 
                            : err.error?.text || 'שגיאה כללית בהעלאה.';

        this.statusMessage = `שגיאת עיבוד: ${serverMessage}`;
      }
    });

    this.fileInput.nativeElement.value = '';
  }
}