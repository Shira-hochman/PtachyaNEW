import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormDataService, FormDto } from '../../services/form-data.service';

// PrimeNG Imports
import { TabsModule } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-forms-management',
  standalone: true,
  imports: [
    CommonModule,
    TabsModule,
    CardModule,
    ButtonModule,
    TagModule,
    ToolbarModule,
    ProgressSpinnerModule,
    TooltipModule
  ],
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
    if (!confirm('האם לאשר את הטופס המבוקש?')) return;

    this.formService.approveForm(form.formId).subscribe({
      next: () => {
        this.pendingForms = this.pendingForms.filter(f => f.formId !== form.formId);
        const approvedForm = { ...form, status: 'Approved' };
        this.approvedForms.unshift(approvedForm);
      },
      error: (err) => alert('שגיאה באישור הטופס. אנא נסה שנית.')
    });
  }

  getFormTypeName(type: string): string {
    const types: { [key: string]: string } = {
      'HEALTH_DECLARATION': 'הצהרת בריאות',
      'DISCOUNT_REQUEST': 'בקשת הנחה'
    };
    return types[type] || type;
  }

  // forms-management.component.ts

getSeverity(status: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
    if (status === 'Approved') {
        return 'success'; // צבע ירוק
    }
    return 'warn'; // צבע כתום - חובה להשתמש ב-'warn' ולא ב-'warning'
}
}