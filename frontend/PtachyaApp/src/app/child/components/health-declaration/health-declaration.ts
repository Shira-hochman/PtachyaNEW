import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router'; 
import { ChildAuthService } from '../../services/child-auth.service';
import { FormService } from '../../services/form.service'; 
import { Child } from '../../../../models/child'; 
import { finalize } from 'rxjs/operators'; // נדרש לכיבוי ה-Loading בצורה בטוחה
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-health-declaration',
  templateUrl: './health-declaration.html',
  styleUrls: ['./health-declaration.css'],
  standalone: true, 
  imports: [CommonModule, ReactiveFormsModule],
  providers: [DatePipe]
})
export class HealthDeclarationComponent implements OnInit, AfterViewInit {
    
  healthDeclarationForm!: FormGroup;
  submitted = false;
  
  // ⭐️ Signal לניהול מצב הטעינה (העיגול המסתובב)
  isLoading = signal(false);

  @ViewChild('parent1SignatureCanvas') canvas1!: ElementRef<HTMLCanvasElement>;
  @ViewChild('parent2SignatureCanvas') canvas2!: ElementRef<HTMLCanvasElement>;
  
  public ctx1!: CanvasRenderingContext2D;
  public ctx2!: CanvasRenderingContext2D;
  private isDrawing = false;
  
  constructor(
    private fb: FormBuilder,
    private authService: ChildAuthService, 
    private router: Router,
    private formService: FormService, 
    private datePipe: DatePipe,
    private cdr: ChangeDetectorRef
  ) {}

  get f() {
    return this.healthDeclarationForm.controls;
  }
  
  ngOnInit(): void {
    this.initForm(); 
    const childData = this.authService.getCurrentChild();
    
    if (childData) {
      this.populateForm(childData); 
    } else {
      this.router.navigate(['/login']); 
    }
  }
goBack(): void {
    // וודא שהנתיב כאן תואם למה שהגדרת ב-Routes עבור PaymentOptions
    this.router.navigate(['/child/main']); 
  }
  ngAfterViewInit(): void {
    if (this.canvas1 && this.canvas1.nativeElement) {
      this.ctx1 = this.canvas1.nativeElement.getContext('2d')!;
      this.setupCanvas(this.ctx1);
    }
    if (this.canvas2 && this.canvas2.nativeElement) {
      this.ctx2 = this.canvas2.nativeElement.getContext('2d')!;
      this.setupCanvas(this.ctx2);
    }
  }
  
  private setupCanvas(ctx: CanvasRenderingContext2D): void {
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#000000';
  }
  
  startDrawing(ctx: CanvasRenderingContext2D, event: MouseEvent | TouchEvent): void {
    this.isDrawing = true;
    const pos = this.getCanvasPosition(ctx.canvas, event);
    ctx.beginPath();
    ctx.moveTo(pos.x, pos.y);
  }

  draw(ctx: CanvasRenderingContext2D, event: MouseEvent | TouchEvent): void {
    if (!this.isDrawing) return;
    const pos = this.getCanvasPosition(ctx.canvas, event);
    ctx.lineTo(pos.x, pos.y);
    ctx.stroke();
  }

  stopDrawing(parent: 1 | 2): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;
    
    const canvas = parent === 1 ? this.canvas1.nativeElement : this.canvas2.nativeElement;
    const dataUrl = canvas.toDataURL('image/png');
    
    if (parent === 1) {
      this.healthDeclarationForm.get('parent1.signature')?.setValue(dataUrl); 
    } else {
      this.healthDeclarationForm.get('parent2.signature')?.setValue(dataUrl); 
    }
  }

  clearSignature(parent: 1 | 2): void {
    const canvas = parent === 1 ? this.canvas1.nativeElement : this.canvas2.nativeElement;
    const ctx = canvas.getContext('2d')!;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    if (parent === 1) {
      this.healthDeclarationForm.get('parent1.signature')?.setValue('');
    } else {
      this.healthDeclarationForm.get('parent2.signature')?.setValue('');
    }
  }

private getCanvasPosition(canvas: HTMLCanvasElement, event: MouseEvent | TouchEvent): { x: number, y: number } {
    const rect = canvas.getBoundingClientRect();
    
    // קבלת הקואורדינטות לפי סוג האירוע (עכבר או מגע)
    const clientX = event instanceof TouchEvent ? event.touches[0].clientX : (event as MouseEvent).clientX;
    const clientY = event instanceof TouchEvent ? event.touches[0].clientY : (event as MouseEvent).clientY;

    // חישוב יחסי הגודל (במקרה שה-CSS מותח את הקנבס)
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    return {
        x: (clientX - rect.left) * scaleX,
        y: (clientY - rect.top) * scaleY
    };
}

// 1. בתוך initForm - הגדרת ברירת המחדל
initForm(): void {
  this.healthDeclarationForm = this.fb.group({
    childDetails: this.fb.group({
      childFirstName: ['', Validators.required],
      childLastName: ['', Validators.required],
      childId: ['', [Validators.required, Validators.pattern('^[0-9]{9}$')]],
      childDob: ['', Validators.required],
      childAddress: ['', Validators.required],
    }),
    formDate: [this.datePipe.transform(new Date(), 'yyyy-MM-dd'), Validators.required], 
    programProvider: ['פתחיה', Validators.required],
    programFramework: ['גן תקשורתי', Validators.required],

    facilityDetails: this.fb.group({
      facilityName: ['', Validators.required],
      // סמל מוסד נמחק מכאן
      facilityOwnership: ['בעלות עמותה פרטית', Validators.required], // ברירת מחדל
      facilityManagerName: ['', Validators.required],
      facilityAddress: ['', Validators.required],
      facilityPhone: ['', [Validators.required, Validators.pattern('^[0-9]{9,10}$')]],
    }),
    
    monthlySelfParticipation: ['', [Validators.required, Validators.pattern('^[0-9]+(\\.[0-9]{1,2})?$')]], 
    noOtherProgramDeclaration: [false, Validators.requiredTrue], 
    
    parent1: this.fb.group({
      name: ['', Validators.required], 
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{9,10}$')]], 
      signature: ['', Validators.required], 
    }),
    parent2: this.fb.group({
      name: [''], 
      phone: [''],
      signature: [''], 
    }),
  });
}

// 2. בתוך populateForm - המילוי האוטומטי
populateForm(child: any): void {
  console.log('נתוני הילד שהתקבלו:', child); // בדיקה בלוג לראות אם יש שם address

  this.healthDeclarationForm.patchValue({
    childDetails: {
      childFirstName: child.firstName,
      childLastName: child.lastName,
      childId: child.idNumber,
      childDob: child.birthDate ? child.birthDate.substring(0, 10) : '',
    },
    parent1: {
      phone: child.phone, 
    }
  });

  // עדכון קבוצת הגן בצורה מרוכזת
  this.healthDeclarationForm.get('facilityDetails')?.patchValue({
    facilityName: child.kindergartenName,
    // חשוב: וודאי שהשם בשרת הוא kindergartenAddress
    facilityAddress: child.kindergartenAddress || child.address, 
    facilityOwnership: 'בעלות עמותה פרטית'
  });

  this.cdr.detectChanges();
}

  // ⭐️ המתודה המעודכנת עם ניהול מצב הטעינה
  onSubmit(): void {
    this.submitted = true;
    if (this.healthDeclarationForm.invalid) {
        alert('נא למלא את כל השדות הנדרשים כראוי, כולל חתימת הורה 1.');
        return;
    }

    // הפעלת האנימציה
    this.isLoading.set(true);

    const formData = this.healthDeclarationForm.getRawValue();

    this.formService.submitHealthDeclaration(formData)
      .pipe(
        // כיבוי האנימציה בסיום (הצלחה או שגיאה)
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
            alert('הטופס נשלח בהצלחה! העתק נשלח למייל שלכם.');
            this.onReset();
        },
        error: (err) => {
            console.error('שגיאה בשליחת הטופס:', err);
            alert('חלה שגיאה בשליחת הטופס. אנא נסו שוב מאוחר יותר.');
        }
      });
  }
  
  onReset(): void {
    this.submitted = false;
    this.healthDeclarationForm.reset({
      programFramework: '',
      facilityOwnership: '',
      noOtherProgramDeclaration: false,
      formDate: this.datePipe.transform(new Date(), 'yyyy-MM-dd') 
    });
    // ניקוי הקנבסים
    this.clearSignature(1);
    this.clearSignature(2);
  }
}