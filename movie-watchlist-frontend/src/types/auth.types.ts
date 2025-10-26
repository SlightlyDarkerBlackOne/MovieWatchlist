export interface User {
  id: number;
  username: string;
  email: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface UserInfo {
  id: number;
  username: string;
  email: string;
  createdAt: string;
}

export interface LoginCredentials {
  usernameOrEmail: string;
  password: string;
}

export interface RegisterData {
  username: string;
  email: string;
  password: string;
}

export interface AuthenticationResult {
  isSuccess: boolean;
  token?: string;
  refreshToken?: string;
  expiresAt?: string;
  errorMessage?: string;
  user?: UserInfo;
}

export interface ForgotPasswordData {
  email: string;
}

export interface ResetPasswordData {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface PasswordResetResponse {
  success: boolean;
  message: string;
}
