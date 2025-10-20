// src/app/models/login.model.ts

export interface LoginRequest {
  Username: string;
  PasswordHash: string;
}

export interface LoginResponse {
  isSuccess: boolean;
  message: string;
}