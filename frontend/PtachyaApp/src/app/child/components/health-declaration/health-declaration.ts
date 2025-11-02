import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common'; // הוספת DatePipe
import { Router } from '@angular/router'; 
import { ChildAuthService } from '../../services/child-auth.service';
import { FormService } from '../../services/form.service'; // ⭐️ ייבוא FormService
import { Child } from '../../../../models/child'; 

@Component({
  selector: 'app-health-declaration',
  templateUrl: './health-declaration.html',
  styleUrls: ['./health-declaration.css'],
  standalone: true, 
  imports: [CommonModule, ReactiveFormsModule],
  providers: [DatePipe] // נדרש אם משתמשים ב-DatePipe ב-TS
})
export class HealthDeclarationComponent implements OnInit, AfterViewInit {
    
  healthDeclarationForm!: FormGroup;
  submitted = false;

  @ViewChild('parent1SignatureCanvas') canvas1!: ElementRef<HTMLCanvasElement>;
  @ViewChild('parent2SignatureCanvas') canvas2!: ElementRef<HTMLCanvasElement>;
  
  public ctx1!: CanvasRenderingContext2D;
  public ctx2!: CanvasRenderingContext2D;
  private isDrawing = false;
  
  constructor(
    private fb: FormBuilder,
    private authService: ChildAuthService, 
    private router: Router,
    private formService: FormService, // ⭐️ הזרקת FormService
    private datePipe: DatePipe // הזרקת DatePipe
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
  
  // ⭐️ לוגיקת Canvas (setup, start, draw, stop, clear, position)
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
    const dataUrl = canvas.toDataURL('image/png'); // שמירה כ-Base64
    
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
    const clientX = (event as MouseEvent).clientX || (event as TouchEvent).touches[0].clientX;
    const clientY = (event as MouseEvent).clientY || (event as TouchEvent).touches[0].clientY;
    
    return {
      x: clientX - rect.left,
      y: clientY - rect.top
    };
  }
  
  // ⭐️ סוף לוגיקת Canvas


  initForm(): void {
    this.healthDeclarationForm = this.fb.group({
        childDetails: this.fb.group({
            childFirstName: ['', Validators.required],
            childLastName: ['', Validators.required],
            childId: ['', [Validators.required, Validators.pattern('^[0-9]{9}$')]],
            childDob: ['', Validators.required],
            childAddress: ['', Validators.required],
        }),
        
        // נשלח כ-ISO string, ה-C# ידע לפרסר ל-DateTime
        formDate: [this.datePipe.transform(new Date(), 'yyyy-MM-dd'), Validators.required], 
        programProvider: ['', Validators.required],
        programFramework: ['', Validators.required],

        facilityDetails: this.fb.group({
            facilityName: ['', Validators.required],
            facilityOwnership: ['', Validators.required],
            facilityManagerName: ['', Validators.required],
            facilityAddress: ['', Validators.required],
            facilityPhone: ['', [Validators.required, Validators.pattern('^[0-9]{9,10}$')]],
        }),
        
        monthlySelfParticipation: ['', [Validators.required, Validators.pattern('^[0-9]+(\.[0-9]{1,2})?$')]], 
        noOtherProgramDeclaration: [false, Validators.requiredTrue], 
        
        parent1: this.fb.group({
            name: ['', Validators.required], 
            phone: ['', [Validators.required, Validators.pattern('^[0-9]{9,10}$')]], 
            signature: ['', Validators.required], // שדה חתימה חובה
        }),
        parent2: this.fb.group({
            name: [''], 
            phone: [''],
            signature: [''], // שדה חתימה אופציונלי
        }),
    });
  }

  populateForm(child: Child): void {
    const nameParts = child.fullName.split(' ');
    const firstName = nameParts.length > 0 ? nameParts[0] : '';
    const lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : '';
    
    this.healthDeclarationForm.patchValue({
        childDetails: {
            childFirstName: firstName,
            childLastName: lastName,
            childId: child.idNumber,
            childDob: child.birthDate.substring(0, 10),
        },
        parent1: {
            phone: child.phone, 
        },
    });
  }

  onSubmit(): void {
      this.submitted = true;
      if (this.healthDeclarationForm.invalid) {
          alert('נא למלא את כל השדות הנדרשים כראוי, כולל חתימת הורה 1.');
          return;
      }

      const formData = this.healthDeclarationForm.getRawValue();

      // קריאה לשירות ושמירת הקובץ
      this.formService.submitHealthDeclaration(formData).subscribe({
          next: (response: Blob) => {
              // ⭐️⭐️⭐️ תיקון קריטי: שינוי הסיומת ל-PDF ⭐️⭐️⭐️
              const url = window.URL.createObjectURL(response);
              const a = document.createElement('a');
              a.href = url;
              a.download = `Health_Declaration_${formData.childDetails.childId}.pdf`; 
              
              document.body.appendChild(a);
              a.click();
              window.URL.revokeObjectURL(url);
              a.remove();
              
              alert('הטופס נשלח, נשמר והורד למחשבך.');
          },
          error: (err) => {
              console.error('שגיאה בשליחת הטופס או יצירת הקובץ:', err);
              alert('שגיאה בשליחת הטופס. אנא נסה שנית.');
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
  }
}
