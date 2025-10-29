// src/models/login.ts (או כל נתיב שמתאים למבנה שלך)

// טיפוסי הנתונים עבור בקשת הכניסה (נשלח לשרת)
export interface LoginRequest {
  Username: string;
  PasswordHash: string; // מכיל את הסיסמה שהוזנה, לפני ה-hashing בצד השרת
}

// טיפוסי הנתונים עבור תגובת השרת (מתקבל מהשרת)
export interface LoginResponse {
  isSuccess: boolean;
  message: string;
  userId: number;
  token?: string; // 🔑 ה-JWT Token המאובטח (שדה קריטי חדש)
}
