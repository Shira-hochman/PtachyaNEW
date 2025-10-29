// src/app/auth/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('auth_token'); // ✅ קבלת הטוקן

  // אם יש טוקן, שכפל את הבקשה והוסף את כותרת Authorization
  if (token) {
    const cloned = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`) // ✅ שליחת הטוקן
    });
    return next(cloned);
  }

  // אם אין טוקן, המשך כרגיל
  return next(req);
};