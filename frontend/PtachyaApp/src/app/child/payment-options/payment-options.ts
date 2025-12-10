import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment-options',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-options.html',
  styleUrls: ['./payment-options.css']
})
export class PaymentOptions {

  constructor(private router: Router) {}

  // כפתור 1: מעבר לטופס הישן (בקשת הנחה)
  navigateToDiscountForm(): void {
    // כאן תכניסי את הנתיב שהיה לך קודם ב-Main
    this.router.navigate(['/child/payment-form']);
  }

  // כפתור 2: מעבר לקומפוננטה החדשה (תשלום ישיר)
 // כפתור 2: תשלום באשראי (ישיר)
  navigateToDirectPayment(): void {
    // ✅ אנו שולחים פרמטר source=options כדי שהדף הבא ידע לאן לחזור
    this.router.navigate(['/child/direct-payment'], { queryParams: { source: 'options' } });
  }

  goBack(): void {
    this.router.navigate(['/child/main']); // או הנתיב לדף הבית שלך
  }
}