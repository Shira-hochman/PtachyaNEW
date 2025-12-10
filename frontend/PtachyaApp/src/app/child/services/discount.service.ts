import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DiscountService {

  // מחיר בסיס לילד לחודש
  private readonly BASE_PRICE = 791.50;

  // BehaviorSubject שומר את המחיר המעודכן ומאפשר לרכיבים אחרים להאזין לשינויים
  private finalPriceSource = new BehaviorSubject<number>(this.BASE_PRICE);
  
  // Observable שהקומפוננטות יירשמו אליו
  currentPrice$ = this.finalPriceSource.asObservable();

  constructor() { }

  /**
   * הפונקציה הראשית לחישוב המחיר לפי הקריטריונים של תשפ"ו
   * @param formData הנתונים שהתקבלו מטופס בקשת ההנחה
   */
  calculateAndSetPrice(formData: any): void {
    let bestBaseDiscount = 0; // אחוז ההנחה הגבוה ביותר מבין הקטגוריות הבסיסיות (לא מצטבר)
    let extraDiscount = 0;    // תוספות מצטברות (מלחמה/מילואים)

    // ------------------------------------------------------------
    // 1. קריטריון כמות ילדים / ילד מוגבל (25% הנחה)
    // ------------------------------------------------------------
    // לפי הכלל: שני ילדים ומעלה בפתחיה, או ילד שני מוגבל במשפחה
    const childrenCount = formData.childrenCount || 0; 
    const isSpecialEdSibling = formData.discountReasons?.otherChildSpecialEd;

    // הנחה: אם יש אח בחינוך מיוחד (לפי הצהרה) או אם יש יותר מ-ילד אחד במשפחה (ילד נוכחי + 1)
    if (childrenCount >= 1 || isSpecialEdSibling) { 
       // הערה: בדקתי childrenCount >= 1 כי זה אומר שיש את הילד הנרשם + עוד ילד בבית, סה"כ 2.
       // אם הכוונה היא ל-2 ילדים *בפתחיה* ספציפית, המערכת תצטרך לבדוק זאת מול ה-DB,
       // כרגע אני מסתמך על הצהרת כמות הילדים בטופס כקריטריון ל"משפחה עם מס' ילדים".
       bestBaseDiscount = Math.max(bestBaseDiscount, 25);
    }

    // ------------------------------------------------------------
    // 2. קריטריון המלצת עו"ס (30% הנחה)
    // ------------------------------------------------------------
    if (formData.discountReasons?.socialWorkerRec) {
      bestBaseDiscount = Math.max(bestBaseDiscount, 30);
    }

    // ------------------------------------------------------------
    // 3. קריטריון סוציו-אקונומי (מבחן הכנסה לנפש)
    // ------------------------------------------------------------
    if (formData.discountReasons?.lowIncome && formData.lowIncomeDetails) {
      const details = formData.lowIncomeDetails;
      
      // חישוב הכנסה משפחתית כוללת
      const income1 = parseFloat(details.spouse1AvgMonthlyIncome) || 0;
      const income2 = parseFloat(details.spouse2AvgMonthlyIncome) || 0;
      const totalHouseholdIncome = income1 + income2;

      // חישוב מספר נפשות במשפחה
      // הורים: אם נשוי = 2, אחרת (גרוש/יחידני/אלמן) = 1
      let parentsCount = (formData.maritalStatus === 'נשוא/ה') ? 2 : 1;
      
      // סה"כ נפשות = הורים + ילדים בחזקה + הילד הנרשם (שהוא חלק מהילדים בחזקה בד"כ, נניח שהcount כולל אותו)
      // לצורך החישוב נניח ש-childrenCount הוא "ילדים *נוספים* עד גיל 18" כפי שכתוב בטופס?
      // הטופס אומר "מספר הילדים עד גיל 18 אשר בחזקתי". נניח שזה כולל את כולם.
      // נוסיף את ההורים למספר הילדים.
      const totalPersons = parentsCount + childrenCount;

      if (totalPersons > 0) {
        const incomePerCapita = totalHouseholdIncome / totalPersons;

        // בדיקת מדרגות הנחה לפי הכללים:
        // 0 עד 1,050 ₪ -> 50%
        if (incomePerCapita >= 0 && incomePerCapita <= 1050) {
          bestBaseDiscount = Math.max(bestBaseDiscount, 50);
        } 
        // 1,050 ₪ עד 2,100 ₪ -> 20%
        else if (incomePerCapita > 1050 && incomePerCapita <= 2100) {
          bestBaseDiscount = Math.max(bestBaseDiscount, 20);
        } 
        // 2,100 ₪ עד 3,000 ₪ -> 10%
        else if (incomePerCapita > 2100 && incomePerCapita <= 3000) {
          bestBaseDiscount = Math.max(bestBaseDiscount, 10);
        }
      }
    }

    // ------------------------------------------------------------
    // 4. תוספת חרבות ברזל / מילואים (10% נוספים מעבר לאחוז המקורי)
    // ------------------------------------------------------------
    if (formData.discountReasons?.warOrReserves) {
      extraDiscount += 10;
    }

    // ------------------------------------------------------------
    // חישוב סופי
    // ------------------------------------------------------------
    
    // סה"כ אחוז הנחה
    let totalDiscountPercent = bestBaseDiscount + extraDiscount;

    // הגבלה לוגית ( שלא יעבור 100%, ולרוב לא נותנים חינם לגמרי, אבל נשאיר לפי החישוב)
    if (totalDiscountPercent > 100) totalDiscountPercent = 100;

    // חישוב הסכום להפחתה
    const discountAmountIS = this.BASE_PRICE * (totalDiscountPercent / 100);
    const finalPriceResult = this.BASE_PRICE - discountAmountIS;

    // הדפסה לקונסול לבדיקה (תוכלי למחוק אח"כ)
    console.log(`Calculation: BaseDiscount=${bestBaseDiscount}%, Extra=${extraDiscount}%, FinalPrice=${finalPriceResult}`);

    // עדכון המחיר הסופי להאזנה
    this.finalPriceSource.next(parseFloat(finalPriceResult.toFixed(2)));
  }

  /**
   * איפוס המחיר למחיר המקורי (למשל ביציאה מהטופס)
   */
  resetPrice(): void {
    this.finalPriceSource.next(this.BASE_PRICE);
  }
}