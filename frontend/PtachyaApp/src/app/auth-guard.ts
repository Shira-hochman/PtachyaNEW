// src/app/auth/auth.guard.ts
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  // 1. בדיקה אם יש אסימון (Token) ב-Local Storage
  const token = localStorage.getItem('auth_token');

  if (token) {
    // ⚠️ ניתן להוסיף כאן בדיקה מורכבת יותר (למשל, האם הטוקן פג תוקף)
    return true; // מאפשר גישה
  } else {
    // 2. אם אין אסימון, מנתב לדף ההתחברות
    router.navigate(['/login']); 
    return false;
  }
};