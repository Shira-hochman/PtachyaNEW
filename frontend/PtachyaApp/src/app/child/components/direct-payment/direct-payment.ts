import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; // ✅ הוספת ייבוא לניהול הזיכרון
import { 
  ReactiveFormsModule, 
  FormBuilder, 
  FormGroup, 
  Validators, 
  AbstractControl, 
  ValidationErrors 
} from '@angular/forms';

// וודאי שהנתיב הזה נכון בהתאם למבנה התיקיות שלך
import { DiscountService } from '../../services/discount.service'; 
import { PaymentService } from '../../services/payment.service'; 

@Component({
  selector: 'app-direct-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './direct-payment.html',
  styleUrls: ['./direct-payment.css']
})
export class DirectPayment implements OnInit, OnDestroy {
  
  paymentForm: FormGroup;
  
  // הגדרת מחיר בסיס (למקרה שאין הנחה או תקלה)
  private readonly BASE_PRICE = 791.50;
  
  // משתנה למחיר הנוכחי שיוצג למשתמש
  currentPrice: number = this.BASE_PRICE;

  private originSource: string = 'form';
  // משתנה לשמירת ההרשמה (Subscription) כדי למנוע דליפת זיכרון
  private priceSubscription!: Subscription;

  constructor(
    private router: Router, 
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private discountService: DiscountService,
    private paymentService: PaymentService
  ) {
    // בניית הטופס עם בדיקות מחמירות
    this.paymentForm = this.fb.group({
      cardName: ['', [Validators.required, Validators.pattern(/^[a-zA-Z\u0590-\u05FF\s]{2,40}$/)]], // רק אותיות (עברית/אנגלית)
      cardNumber: ['', [Validators.required, this.luhnValidator]], // בדיקת אלגוריתם כרטיס אשראי
      expiry: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/([0-9]{2})$/), this.expiryDateValidator]], // MM/YY
      cvv: ['', [Validators.required, Validators.pattern(/^[0-9]{3,4}$/)]], // 3 או 4 ספרות
      ownerId: ['', [Validators.required, this.israeliIdValidator]] // בדיקת תקינות ת.ז.
    });
  }

 ngOnInit(): void {
    // 1. קבלת המחיר מהשירות
    this.priceSubscription = this.discountService.currentPrice$.subscribe(price => {
      this.currentPrice = price;
    });

    // 2. ✅ בדיקה מאיפה הגענו (דרך ה-URL)
    // אנו בודקים אם יש פרמטר בשם 'source' בכתובת
    const source = this.route.snapshot.queryParams['source'];
    if (source) {
      this.originSource = source;
    }
  }

  ngOnDestroy(): void {
    // ✅ שחרור הזיכרון ביציאה מהעמוד (חשוב מאוד!)
    if (this.priceSubscription) {
      this.priceSubscription.unsubscribe();
    }
  }

  // --- Validators (בדיקות תקינות) ---

  // בדיקת אלגוריתם לון (Luhn) לכרטיסי אשראי
  luhnValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    let value = control.value.replace(/\D/g, ''); // הסרת רווחים ומקפים
    if (value.length < 13 || value.length > 16) return { invalidLength: true };

    let sum = 0;
    let shouldDouble = false;
    for (let i = value.length - 1; i >= 0; i--) {
      let digit = parseInt(value.charAt(i));
      if (shouldDouble) {
        if ((digit *= 2) > 9) digit -= 9;
      }
      sum += digit;
      shouldDouble = !shouldDouble;
    }
    return (sum % 10 === 0) ? null : { invalidCard: true };
  }

  // בדיקת תקינות תעודת זהות ישראלית
  israeliIdValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    let id = String(control.value).trim();
    if (id.length > 9 || isNaN(Number(id))) return { invalidId: true };
    
    id = id.padStart(9, '0');
    let sum = 0, incNum;
    for (let i = 0; i < 9; i++) {
      incNum = Number(id[i]) * ((i % 2) + 1);
      sum += (incNum > 9) ? incNum - 9 : incNum;
    }
    return (sum % 10 === 0) ? null : { invalidId: true };
  }

  // בדיקת תוקף (עתידי)
  expiryDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const parts = control.value.split('/');
    if (parts.length !== 2) return { invalidDate: true };

    const month = parseInt(parts[0], 10);
    const year = parseInt('20' + parts[1], 10); // המרה ל-20XX
    const now = new Date();
    const currentMonth = now.getMonth() + 1;
    const currentYear = now.getFullYear();

    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      return { pastDate: true };
    }
    return null;
  }

  // --- Form Formatting ---

  // פונקציה לעיצוב אוטומטי של מספר הכרטיס (מוסיפה רווחים)
  formatCardNumber(event: any): void {
    let input = event.target.value.replace(/\D/g, '').substring(0, 16);
    input = input != '' ? input.match(/.{1,4}/g)?.join(' ') : '';
    this.paymentForm.controls['cardNumber'].setValue(input, { emitEvent: false });
  }

  // פונקציה לעיצוב אוטומטי של תוקף (מוסיפה סלאש)
  formatExpiry(event: any): void {
    let input = event.target.value.replace(/\D/g, '').substring(0, 4);
    if (input.length >= 2) {
      input = input.substring(0, 2) + '/' + input.substring(2);
    }
    this.paymentForm.controls['expiry'].setValue(input, { emitEvent: false });
  }

  // --- Actions ---

// processPayment(): void {
//   if (this.paymentForm.valid) {
//     const paymentPayload = {
//       ...this.paymentForm.value,
//       amount: this.currentPrice
//     };

//     // הגדרת סוגים (any או ממשק ייעודי) כדי למנוע את שגיאת TS7006
//     this.paymentService.submitToBackend(paymentPayload).subscribe({
//       next: (response: any) => { 
//         alert(`תשלום על סך ₪${this.currentPrice.toFixed(2)} בוצע בהצלחה!`);
//         this.discountService.resetPrice();
//         this.router.navigate(['/child/main']);
//       },
//       error: (err: any) => {
//         console.error('Payment error:', err);
//         alert('חלה שגיאה בביצוע התשלום. נא נסו שוב.');
//       }
//     });
//   } else {
//     this.paymentForm.markAllAsTouched();
//   }
// }
processPayment(): void {
  if (this.paymentForm.valid) {
    // חילוץ ה-ID מה-Token (בהנחה שהוא שמור ב-localStorage)
    const token = localStorage.getItem('token'); 
    let childId = 1; // ברירת מחדל למקרה חירום, שאי שזה ID שקיים ב-DB שלך!

    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // לפי ה-Header ששלחת, השדה ב-Token נקרא "nameid"
      childId = +payload.nameid; 
    }

    const paymentPayload = {
      ChildId: childId, // ה-ID האמיתי של הילד
      Amount: 1.00,
      CardNumber: this.paymentForm.value.cardNumber.replace(/\s/g, ''),
      Expiry: this.paymentForm.value.expiry.replace('/', ''),
      Cvv: this.paymentForm.value.cvv,
      HolderId: this.paymentForm.value.ownerId,
      HolderName: this.paymentForm.value.cardName
    };

   

    this.paymentService.submitToBackend(paymentPayload).subscribe({
      next: (response: any) => { 
        alert(`בדיקת מערכת: תשלום על סך ₪1.00 בוצע בהצלחה!`);
        this.discountService.resetPrice();
        this.router.navigate(['/child/main']);
      },
      error: (err: any) => {
        // הדפסת השגיאה המפורטת מהשרת כדי להבין מה נכשל ב-Validation
        console.error('Detailed Server Error:', err.error);
        alert('חלה שגיאה בביצוע התשלום. בדקי את הנתונים וסנכרון ה-DTO.');
      }
    });
  } else {
    this.paymentForm.markAllAsTouched();
  }
}
goBack(): void {
    if (this.originSource === 'options') {
      // אם הגענו ממסך האפשרויות - נחזור לשם
      this.router.navigate(['/child/payment-options']);
    } else {
      // אחרת (ברירת מחדל) - נחזור לטופס ההנחה
      this.router.navigate(['/child/payment-form']);
    }
  }
}