import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminDto, CreateAdminDto } from '../../services/admin.service';

// PrimeNG Imports
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ToolbarModule } from 'primeng/toolbar';
import { PasswordModule } from 'primeng/password';
import { MessageService, ConfirmationService } from 'primeng/api'; // ✅ הוספת ConfirmationService
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog'; // ✅ הוספת המודול לדיאלוג אישור

@Component({
  selector: 'app-admin-management',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, DialogModule, ToolbarModule, PasswordModule,
    ToastModule, ConfirmDialogModule // ✅ חובה להוסיף גם כאן
  ],
  providers: [MessageService, ConfirmationService], // ✅ חובה בפרובידרים
  templateUrl: './admin-management.html',
  styleUrls: ['./admin-management.css']
})
export class AdminManagementComponent implements OnInit {

  admins: AdminDto[] = [];
  isAddModalOpen: boolean = false;
  newAdmin: CreateAdminDto = { username: '', password: '' };

  constructor(
    private adminService: AdminService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService // ✅ הזרקה בקונסטרקטור
  ) {}

  ngOnInit() {
    this.loadAdmins();
  }

  loadAdmins() {
    this.adminService.getAllAdmins().subscribe({
      next: (data) => this.admins = data,
      error: (err) => console.error(err)
    });
  }

 // בתוך הקלאס AdminManagementComponent

openAddModal() {
  // ✅ תיקון: איפוס מוחלט של האובייקט כדי שהשדות יהיו ריקים
  this.newAdmin = { username: '', password: '' }; 
  this.isAddModalOpen = true;
}

  saveNewAdmin() {
     // ... (הקוד הקודם שלך נשאר זהה) ...
     if (!this.newAdmin.username || !this.newAdmin.password) {
        this.messageService.add({severity:'warn', summary:'שגיאה', detail:'יש למלא את כל השדות'});
        return;
     }
     
     this.adminService.addAdmin(this.newAdmin).subscribe({
        next: () => {
           this.messageService.add({severity:'success', summary:'הצלחה', detail:'מנהל נוסף בהצלחה'});
           this.isAddModalOpen = false;
           this.loadAdmins();
        },
        error: () => this.messageService.add({severity:'error', summary:'תקלה', detail:'שגיאה ביצירת מנהל'})
     });
  }

  // ✅ פונקציה חדשה למחיקה
  deleteAdmin(admin: AdminDto) {
    this.confirmationService.confirm({
      message: `האם אתה בטוח שברצונך למחוק את המנהל <b>${admin.username}</b>?`,
      header: 'אישור מחיקה',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'מחק',
      rejectLabel: 'ביטול',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        // ביצוע המחיקה בפועל
        this.adminService.deleteAdmin(admin.userId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'נמחק', detail: 'המנהל הוסר בהצלחה' });
            // הסרה מהרשימה המקומית כדי לא להעמיס בקריאת שרת נוספת
            this.admins = this.admins.filter(a => a.userId !== admin.userId);
          },
          error: () => {
             this.messageService.add({ severity: 'error', summary: 'שגיאה', detail: 'לא ניתן למחוק את המנהל' });
          }
        });
      }
    });
  }
}