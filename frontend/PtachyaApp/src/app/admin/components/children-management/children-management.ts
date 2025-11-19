import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { HttpClientModule } from '@angular/common/http'; 
import { FormsModule } from '@angular/forms'; // ⬅️ ייבוא לשימוש ב-ngModel לחיפוש/סינון
import { ChildDataService, ChildDto } from '../../services/child-data.service';
import { RouterLink } from '@angular/router'; // ⬅️ לניווט למסך הוספת ילד

@Component({
  selector: 'app-children-management', // ⬅️ שם הסלקטור החדש
  standalone: true, 
  imports: [CommonModule, HttpClientModule, FormsModule, RouterLink], // ⬅️ הוספנו FormsModule ו-RouterLink
  templateUrl: './children-management.html', // ⬅️ נניח ששם קובץ ה-HTML שונה/עודכן
  styleUrl: './children-management.css'
})
export class ChildrenManagementComponent implements OnInit { // ⬅️ שם הקלאס החדש
  
  children: ChildDto[] | null = null; 
  isLoading: boolean = false;
  errorMessage: string | null = null;
  
  // ⭐️ שדות חדשים לחיפוש וסינון
  searchTerm: string = '';
  selectedKindergartenId: number | null = null;
  kindergartens: any[] = [{ id: 1, name: 'גן אלון' }, { id: 2, name: 'גן ברוש' }]; // נתונים לדוגמה
  
  constructor(private childDataService: ChildDataService) { }

  ngOnInit() {
    this.loadChildren(); // ⭐️ טוענים נתונים אוטומטית בכניסה למסך
  }

  /**
   * טוען את רשימת הילדים מהשרת עם פרמטרים של חיפוש וסינון
   */
  loadChildren(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.children = null; 
    
    // 💡 בפרויקט אמיתי, היינו קוראים לשירות חדש:
    // this.childDataService.getFilteredChildren(this.searchTerm, this.selectedKindergartenId).subscribe({ ... });

    // כרגע נשתמש בקיים ונבצע סינון בסיסי בצד לקוח לצורך הדגמה
    this.childDataService.getAllChildren().subscribe({
        next: (data: ChildDto[]) => {
            let filteredData = data;
            
            // ⭐️ סינון בסיסי לפי טקסט (כאשר זה בצד לקוח)
            if (this.searchTerm) {
                const term = this.searchTerm.toLowerCase();
                filteredData = filteredData.filter(child => 
                    child.firstName.toLowerCase().includes(term) ||
                    child.lastName.toLowerCase().includes(term) ||
                    child.idNumber.includes(term)
                );
            }
            // ⭐️ סינון לפי גן
           // ילדים עם שגיאת TS2367 (השוואת סוגים לא תואמים)
// ⭐️ סינון לפי גן - הבלוק המתוקן
            if (this.selectedKindergartenId !== null) {
                // המרת הערך הנבחר מה-HTML למספר
                const selectedId = Number(this.selectedKindergartenId);
                
                filteredData = filteredData.filter(child => {
                    // 💡 כדי למנוע שגיאות סוג, אנו ממירים גם את השדה באובייקט הילד למספר
                    // אנו משתמשים ב-== במקום === כדי לאפשר השוואה גם אם הטיפוסים שונים קלות,
                    // או שפשוט נמיר את שניהם למספרים:
                    return Number(child.kindergartenId) === selectedId;
                });
            }
            
            this.children = filteredData;
            this.isLoading = false;
            this.errorMessage = filteredData.length === 0 ? 'לא נמצאו ילדים תואמים לחיפוש.' : null;
        },
        error: (err: any) => {
            console.error('Failed to load children:', err);
            this.errorMessage = 'שגיאה בטעינת הנתונים.';
            this.isLoading = false;
        }
    });
  }

  // ⭐️ פונקציה לטיפול בלחיצה על "עריכה"
  editChild(childId: number): void {
    alert(`פתח חלון עריכה עבור ילד ID: ${childId}`);
    // 💡 כאן נפעיל מודל קופץ (Modal) עם טופס העריכה
  }
}