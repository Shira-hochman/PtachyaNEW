import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormDataService, FormDto } from '../../services/form-data.service';

@Component({
  selector: 'app-forms-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './forms-management.html',
  styleUrls: ['./forms-management.css']
})
export class FormsManagementComponent implements OnInit {
  
  pendingForms: FormDto[] = [];
  approvedForms: FormDto[] = [];
  isLoading = true;

  constructor(private formService: FormDataService) {}

  ngOnInit() {
    this.loadAllForms();
  }

  loadAllForms() {
    this.isLoading = true;
    
    // טעינת ממתינים
    this.formService.getPendingForms().subscribe({
      next: (data) => this.pendingForms = data,
      error: (err) => console.error('Error loading pending forms:', err)
    });

    // טעינת מאושרים
    this.formService.getApprovedForms().subscribe({
      next: (data) => {
        this.approvedForms = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading approved forms:', err);
        this.isLoading = false;
      }
    });
  }

  approveForm(form: FormDto) {
    if (!confirm('האם לאשר את הטופס?')) return;

    this.formService.approveForm(form.formId).subscribe({
      next: () => {
        // 1. הסרה מרשימת הממתינים
        this.pendingForms = this.pendingForms.filter(f => f.formId !== form.formId);
        
        // 2. עדכון הסטטוס והוספה לרשימת המאושרים (כדי שיראו מיד ללא רענון)
        // יצירת עותק מעודכן כדי לא לשנות את המקור ישירות אם יש הפניות
        const approvedForm = { ...form, status: 'Approved' };
        this.approvedForms.unshift(approvedForm); // הוספה לראש הרשימה
      },
      error: (err) => alert('שגיאה באישור הטופס')
    });
  }

  getFormTypeName(type: string): string {
    if (type === 'HEALTH_DECLARATION') return 'הצהרת בריאות';
    if (type === 'DISCOUNT_REQUEST') return 'בקשת הנחה';
    return type;
  }
}