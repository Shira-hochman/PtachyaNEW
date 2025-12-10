import { ChangeDetectionStrategy, Component, ElementRef, OnInit, ViewChild, AfterViewInit, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray, AbstractControl, ValidatorFn, ValidationErrors } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { FormService } from '../../services/form.service'; // ⭐️ ייבוא FormService
import { ChildAuthService } from '../../services/child-auth.service';
import { Child } from '../../../../models/child';
import { DiscountService } from '../../services/discount.service';
import { Router } from '@angular/router'; // ✅ חובה לניווט
import { finalize } from 'rxjs/operators';

// ממשק פשוט לילד בחזקת ההורה (Child in Custody)
interface ChildInCustody {
  firstName: string;
  lastName: string;
  id: string;
}

// ולידטור מותאם אישית: לפחות תיבת סימון אחת נבחרה
const atLeastOneReasonValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const reasons = control as FormGroup;
  // בודק רק את הסיבות הקיימות כעת
  const isChecked = Object.keys(reasons.controls).some(key => reasons.get(key)?.value);
  return isChecked ? null : { atLeastOneRequired: true };
};


// קומפוננטת בקשת ההנחה
@Component({
  selector: 'app-payment-form',
  templateUrl: './payment-form.html',
  styleUrls: ['./payment-form.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  providers: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentForm implements OnInit, AfterViewInit {
  // נכסי הקומפוננטה
  discountRequestForm!: FormGroup;
  submitted = signal(false);
  submissionMessage = signal<string | null>(null);
  date = signal<string | null>(null);
isLoading = signal(false);
  // נכסי חתימה
  @ViewChild('parentSignatureCanvas') canvas!: ElementRef<HTMLCanvasElement>;
  public ctx!: CanvasRenderingContext2D;
  private isDrawing = false;

  filesToUpload: { [key: string]: File | null } = {
    lowIncomeDocsUploaded: null,
    otherSpecialEdDocsUploaded: null,
    socialWorkerDocsUploaded: null,
  };

  // נתוני בחירה
  maritalStatuses = ['נשוא/ה', 'הורה יחידני', 'גרוש/ה'];
  employmentStatuses = ['שכיר/ה', 'עצמאי/ת', 'לא עובד/ת'];

  // ⭐️ הזרקת FormService ו-Router
  constructor(
    private fb: FormBuilder,
    private datePipe: DatePipe,
    private formService: FormService, // ⭐️ הזרקת השירות
    private authService: ChildAuthService,
    private router: Router, // ✅ הזרקת Router
    private discountService: DiscountService // ✅ הזרקת DiscountService
  ) { }

  // מחזיר את קבוצת הסיבות לבקשת הנחה
  get discountReasons(): FormGroup {
    return this.discountRequestForm.get('discountReasons') as FormGroup;
  }

  // מחזיר את מערך הילדים בחזקת ההורה
  get childrenInCustody(): FormArray {
    return this.discountRequestForm.get('childrenInCustody') as FormArray;
  }

  // מחזיר האם שדה ספציפי אינו תקין ונשלח
  isInvalid(path: string): boolean {
    const control = this.discountRequestForm.get(path);
    return !!control?.invalid && (!!control?.dirty || !!control?.touched || this.submitted());
  }

  // PaymentForm.ts

ngOnInit(): void {
    this.date.set(this.datePipe.transform(new Date(), 'yyyy-MM-dd'));
    this.initForm();
    this.setupConditionalLogic();
    
    // ⭐️⭐️⭐️ שליפה ומילוי אוטומטי ⭐️⭐️⭐️
    const childData = this.authService.getCurrentChild();
    if (childData) {
        this.populateForm(childData); 
    } else {
        // אם אין ילד מאומת, ניתן לנתב או להשאיר למילוי ידני
        console.warn('No authenticated child found. Discount form must be filled manually.');
    }
    // ⭐️⭐️⭐️ סוף שליפה ומילוי אוטומטי ⭐️⭐️⭐️
}
  ngAfterViewInit(): void {
    // אתחול Canvas רק אם האלמנט קיים
    if (this.canvas && this.canvas.nativeElement) {
      this.ctx = this.canvas.nativeElement.getContext('2d')!;
      this.setupCanvas(this.ctx);
    }
  }

  // יצירת קבוצת בקרה לילד בחזקת ההורה
  createChildInCustody(): FormGroup {
    return this.fb.group({
      firstName: [''],
      lastName: [''],
      id: ['', Validators.pattern('^[0-9]{9}$')],
    });
  }

  // עדכון מערך הילדים לפי המספר שהוזן
  updateChildrenInCustodyArray(count: number | null): void {
    if (count === null || count < 0) return;

    // המגבלה מוגדלת ל-18
    const maxRows = Math.min(count, 18);

    while (this.childrenInCustody.length < maxRows) {
      this.childrenInCustody.push(this.createChildInCustody());
    }

    while (this.childrenInCustody.length > maxRows) {
      this.childrenInCustody.removeAt(this.childrenInCustody.length - 1);
    }

    // ודא שהולידציה של ה-FormArray מתעדכנת
    this.childrenInCustody.updateValueAndValidity();
  }

  // אתחול הטופס
  initForm(): void {
    this.discountRequestForm = this.fb.group({
      // ... שאר השדות
      declarantName: ['', Validators.required],
      declarantId: ['', [Validators.required, Validators.pattern('^[0-9]{9}$')]],
      maritalStatus: ['', Validators.required],
      childrenCount: [0, [Validators.required, Validators.min(0)]],
      childrenInCustody: this.fb.array([]),
      
      studentDetails: this.fb.group({
        studentName: ['', Validators.required],
        studentId: ['', [Validators.required, Validators.pattern('^[0-9]{9}$')]],
        kindergarten: ['', Validators.required],
        city: ['', Validators.required],
      }),

      // ✅ עדכון: הוספת warOrReserves ל-FormGroup
      discountReasons: this.fb.group({
        lowIncome: [false],
        otherChildSpecialEd: [false],
        socialWorkerRec: [false],
        warOrReserves: [false], // ✅ השדה החדש
      }, { validators: atLeastOneReasonValidator }), // וודאי שה-validator הזה מוגדר למעלה בקובץ

      // ... שאר הקבוצות (lowIncomeDetails, requiredDocuments וכו')
      lowIncomeDetails: this.fb.group({
         // ... שדות ההכנסה
         spouse1Status: [''], spouse1AvgMonthlyIncome: [null], spouse1Total3Months: [null],
         spouse2Status: [''], spouse2AvgMonthlyIncome: [null], spouse2Total3Months: [null],
      }),
      requiredDocuments: this.fb.group({
        lowIncomeDocsUploaded: [''],
        otherSpecialEdDocsUploaded: [''],
        socialWorkerDocsUploaded: [''],
      }),
      reasoning: ['', Validators.required],
      formDate: [this.date()],
      parentSignature: ['', Validators.required],
    });

    this.updateChildrenInCustodyArray(0);
  }
  // לוגיקה מותנית לשדות
  setupConditionalLogic(): void {
    // 1. פרטי הכנסה והעלאת מסמכים נדרשים
    this.discountReasons.get('lowIncome')?.valueChanges.subscribe(isLowIncome => {
      const detailsGroup = this.discountRequestForm.get('lowIncomeDetails') as FormGroup;
      const docControl = this.discountRequestForm.get('requiredDocuments.lowIncomeDocsUploaded')!;

      this.removeRequiredValidators(detailsGroup);
      docControl.clearValidators();

      if (isLowIncome) {
        detailsGroup.enable();
        this.addRequiredValidators(detailsGroup);
        // ולידציה: דורש שהערך לא יהיה ריק
        docControl.setValidators(Validators.required);
      } else {
        detailsGroup.disable();
        detailsGroup.reset({
          spouse1Status: '', spouse1AvgMonthlyIncome: null, spouse1Total3Months: null,
          spouse2Status: '', spouse2AvgMonthlyIncome: null, spouse2Total3Months: null,
        });
        docControl.setValue('');
      }
      detailsGroup.updateValueAndValidity();
      docControl.updateValueAndValidity();
    });

    // 2. אישור לימודים (חינוך מיוחד אחר)
    this.discountReasons.get('otherChildSpecialEd')?.valueChanges.subscribe(isSpecialEdChild => {
      const docControl = this.discountRequestForm.get('requiredDocuments.otherSpecialEdDocsUploaded')!;
      docControl.clearValidators();

      if (isSpecialEdChild) {
        docControl.setValidators(Validators.required); // מסמך חובה
      } else {
        docControl.setValue('');
      }
      docControl.updateValueAndValidity();
    });

    // 3. אסמכתא מעובדת סוציאלית
    this.discountReasons.get('socialWorkerRec')?.valueChanges.subscribe(isSocialWorkerRec => {
      const docControl = this.discountRequestForm.get('requiredDocuments.socialWorkerDocsUploaded')!;
      docControl.clearValidators();

      if (isSocialWorkerRec) {
        docControl.setValidators(Validators.required); // מסמך חובה
      } else {
        docControl.setValue('');
      }
      docControl.updateValueAndValidity();
    });

    // הרצה ראשונית
    this.discountReasons.controls['lowIncome'].updateValueAndValidity();
    this.discountReasons.controls['otherChildSpecialEd'].updateValueAndValidity();
    this.discountReasons.controls['socialWorkerRec'].updateValueAndValidity();
  }

  // פונקציות עזר להוספה והסרת Validators
  private addRequiredValidators(group: FormGroup, keys?: string[]): void {
    const controlsToUpdate = keys || Object.keys(group.controls);
    controlsToUpdate.forEach(key => {
      const control = group.get(key);
      if (control) {
        const currentValidators = control.validator ? [control.validator] : [];
        const hasRequired = currentValidators.some(v => v === Validators.required);
        if (!hasRequired) {
          control.setValidators([Validators.required, ...currentValidators]);
        }
        control.updateValueAndValidity({ emitEvent: false });
      }
    });
  }

  private removeRequiredValidators(group: FormGroup, keys?: string[]): void {
    const controlsToUpdate = keys || Object.keys(group.controls);
    controlsToUpdate.forEach(key => {
      const control = group.get(key);
      if (control) {
        // הסינון הזה לא תמיד עובד נכון עם setValidators()
        control.clearValidators();
        // כיוון שהסרנו את כל ה-Validators, נוודא שאין null/undefined
        control.setValidators(null);
        control.updateValueAndValidity({ emitEvent: false });
      }
    });
  }

  // לוגיקת Canvas - הגדרה
  private setupCanvas(ctx: CanvasRenderingContext2D): void {
    ctx.lineWidth = 3;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#000000';
  }

  // לוגיקת Canvas - התחלת ציור
  startDrawing(ctx: CanvasRenderingContext2D, event: MouseEvent | TouchEvent): void {
    this.isDrawing = true;
    const pos = this.getCanvasPosition(ctx.canvas, event);
    ctx.beginPath();
    ctx.moveTo(pos.x, pos.y);
    event.preventDefault();
  }

  // לוגיקת Canvas - ציור
  draw(ctx: CanvasRenderingContext2D, event: MouseEvent | TouchEvent): void {
    if (!this.isDrawing) return;
    const pos = this.getCanvasPosition(ctx.canvas, event);
    ctx.lineTo(pos.x, pos.y);
    ctx.stroke();
    event.preventDefault();
  }

  // לוגיקת Canvas - הפסקת ציור ושמירה
  stopDrawing(): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;

    const canvas = this.canvas.nativeElement;
    const dataUrl = this.ctx.getImageData(0, 0, canvas.width, canvas.height).data.some(channel => channel !== 0) ?
      canvas.toDataURL('image/png') :
      '';

    this.discountRequestForm.get('parentSignature')?.setValue(dataUrl);
    this.discountRequestForm.get('parentSignature')?.markAsDirty();
  }

  // לוגיקת Canvas - ניקוי
  clearSignature(): void {
    const canvas = this.canvas.nativeElement;
    const ctx = canvas.getContext('2d')!;
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    this.discountRequestForm.get('parentSignature')?.setValue('');
    this.discountRequestForm.get('parentSignature')?.markAsDirty();
  }

  // לוגיקת Canvas - חישוב מיקום
  private getCanvasPosition(canvas: HTMLCanvasElement, event: MouseEvent | TouchEvent): { x: number, y: number } {
    const rect = canvas.getBoundingClientRect();
    const clientX = (event as MouseEvent).clientX !== undefined ? (event as MouseEvent).clientX : (event as TouchEvent).touches[0].clientX;
    const clientY = (event as MouseEvent).clientY !== undefined ? (event as MouseEvent).clientY : (event as TouchEvent).touches[0].clientY;

    // קנה מידה להתאמת רזולוציית ה-Canvas לגודל ה-DOM
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    return {
      x: (clientX - rect.left) * scaleX,
      y: (clientY - rect.top) * scaleY
    };
  }

  // מטפל בבחירת קובץ אמיתית
  // מטפל בבחירת קובץ אמיתית
  onFileChange(event: Event, controlName: string): void {
    const input = event.target as HTMLInputElement;
    const control = this.discountRequestForm.get(`requiredDocuments.${controlName}`);

    if (control) {
      if (input.files && input.files.length > 0) {
        const file = input.files[0];
        // ⭐️⭐️⭐️ שמירת אובייקט הקובץ ⭐️⭐️⭐️
        this.filesToUpload[controlName] = file;

        // שמירת שם הקובץ רק לצורך תצוגה
        control.setValue(file.name);
        this.submissionMessage.set(`✅ קובץ "${file.name}" נבחר בהצלחה.`);
      } else {
        // ⭐️⭐️⭐️ איפוס אובייקט הקובץ ⭐️⭐️⭐️
        this.filesToUpload[controlName] = null;
        control.setValue('');
        this.submissionMessage.set(null);
      }
      control.markAsDirty();
      control.markAsTouched();
      control.updateValueAndValidity();
    }
  }

  // PaymentForm.ts

// ⭐️⭐️⭐️ מתודה חדשה: מילוי טופס ההנחה מנתוני הילד השמורים ⭐️⭐️⭐️
populateForm(child: Child): void {
    this.discountRequestForm.patchValue({
        // 1. פרטי מגיש הבקשה (הורה/מצהיר)
        // הנחה: ההורה המחובר הוא מגיש הבקשה
       
        // 2. פרטי התלמיד
        studentDetails: {
            studentName: child.firstName,
            studentId: child.idNumber,
            // Kindergarten ו-City לא קיימים במודל Child הנוכחי. 
            // אם הם מגיעים עם ChildDto, הם ימולאו:
            // kindergarten: child.kindergarten.name, 
            // city: child.kindergarten.city,
            // אם הם לא קיימים, תצטרך לשלוף אותם בנפרד!
        }
        // ... אם יש שדות נוספים מהילד שצריך למלא (כמו טלפון/אימייל)
    });
}
  // שליחת הטופס
  // שליחת הטופס (המתודה המעודכנת)
 onSubmit(): void {
    this.submitted.set(true);
    this.submissionMessage.set(null);

    this.discountRequestForm.markAllAsTouched();

    if (this.discountRequestForm.invalid) {
      console.error('Form is invalid:', this.discountRequestForm.errors);
      this.submissionMessage.set('🔴 שגיאה בשליחה: נא תקן את כל השגיאות המסומנות בטופס.');
      return;
    }

    // ✅ הפעלת הספינר
    this.isLoading.set(true);

    // הכנת הנתונים (אותו קוד בדיוק)
    const cleanChildren = this.childrenInCustody.controls
      .map(control => control.getRawValue())
      .filter(child => child.firstName || child.lastName || child.id);

    const formJsonData = {
      ...this.discountRequestForm.getRawValue(),
      childrenInCustody: cleanChildren,
    };

    const formData = new FormData();
    formData.append('data', JSON.stringify(formJsonData));
    
    if (this.filesToUpload['lowIncomeDocsUploaded']) formData.append('lowIncomeFile', this.filesToUpload['lowIncomeDocsUploaded'] as File);
    if (this.filesToUpload['otherSpecialEdDocsUploaded']) formData.append('specialEdFile', this.filesToUpload['otherSpecialEdDocsUploaded'] as File);
    if (this.filesToUpload['socialWorkerDocsUploaded']) formData.append('socialWorkerFile', this.filesToUpload['socialWorkerDocsUploaded'] as File);

    // שליחה לשרת
    this.formService.submitDiscountRequest(formData)
      .pipe(
        // ✅ finalize מבטיח שהקוד ירוץ גם בהצלחה וגם בכישלון
        // במקרה של שגיאה - נכבה את הספינר.
        // במקרה של הצלחה - נשאיר אותו דולק עד המעבר דף (כדי למנוע "קפיצה" ויזואלית)
        finalize(() => {
           // אנו לא מכבים כאן את isLoading בכוונה במקרה של הצלחה,
           // כי אנחנו מיד מנווטים.
           // נכבה אותו רק ב-error למטה.
        })
      )
      .subscribe({
      next: (response: Blob) => {
        
        // לוגיקת הורדה...
        const url = window.URL.createObjectURL(response);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Discount_Request_${formJsonData.studentDetails.studentId}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();

        // עדכון השירות וחישוב מחיר
        this.discountService.calculateAndSetPrice(formJsonData);

        // עדכון טקסט ההודעה (למרות שהמשתמש אולי לא יראה אותה בגלל הספינר, זה טוב לדיבאג)
        this.submissionMessage.set('✅ הטופס נקלט. מעביר לתשלום...');

        // מעבר דף
        setTimeout(() => {
           // מכבים את הספינר רגע לפני המעבר (אופציונלי, הדפדפן ממילא ירענן)
           this.isLoading.set(false); 
           this.router.navigate(['/child/direct-payment']);
        }, 1500);
      },
      error: (err) => {
        console.error('Error submitting form:', err);
        // ✅ במקרה של שגיאה - חובה לכבות את הספינר כדי שהמשתמש יראה את הודעת השגיאה
        this.isLoading.set(false);
        this.submissionMessage.set('🔴 ארעה שגיאה בשליחת הטופס. נסה שנית מאוחר יותר.');
      }
    });
  }

  // איפוס הטופס
  onReset(): void {
    this.submitted.set(false);
    this.submissionMessage.set(null);
    this.clearSignature();
    this.discountRequestForm.reset();

    // איפוס ערכי ברירת המחדל ותאריך
    this.discountRequestForm.patchValue({
      maritalStatus: '',
      childrenCount: 0,
      formDate: this.date(),
      discountReasons: {
        lowIncome: false,
        otherChildSpecialEd: false,
        socialWorkerRec: false,
      },
      requiredDocuments: {
        lowIncomeDocsUploaded: '', // איפוס לשם הקובץ הריק
        otherSpecialEdDocsUploaded: '',
        socialWorkerDocsUploaded: '',
      }
    });

    this.setupConditionalLogic();
    this.updateChildrenInCustodyArray(0);
    this.discountRequestForm.markAsPristine();
    this.discountRequestForm.markAsUntouched();
  }
}