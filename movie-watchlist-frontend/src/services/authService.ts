import api from './api';
import { API_ENDPOINTS } from '../utils/constants';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  UserInfo
} from '../types/auth.types';
import { AxiosError } from '../types/error.types';

class AuthService {
  async login(credentials: LoginCredentials): Promise<AuthenticationResult> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.LOGIN, credentials);
      const { user, expiresAt } = response.data;
      return { isSuccess: true, user, expiresAt } as AuthenticationResult;
    } catch (error) {
      const axiosError = error as AxiosError;
      return {
        isSuccess: false,
        errorMessage: String(axiosError.response?.data?.error || axiosError.response?.data?.message || 'Login failed')
      };
    }
  }

  async register(userData: RegisterData): Promise<AuthenticationResult> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.REGISTER, userData);
      const { user, expiresAt } = response.data;
      return { isSuccess: true, user, expiresAt } as AuthenticationResult;
    } catch (error) {
      const axiosError = error as AxiosError;
      let errorMessage = 'Registration failed';
      if (axiosError.response?.data?.error) {
        errorMessage = String(axiosError.response.data.error);
      } else if (axiosError.response?.data?.message) {
        errorMessage = String(axiosError.response.data.message);
      } else if (axiosError.response?.data?.errors) {
        const errors = axiosError.response.data.errors;
        const errorMessages = Object.values(errors).flat();
        errorMessage = errorMessages.join(', ');
      } else if (axiosError.message) {
        errorMessage = axiosError.message;
      }
      
      return { isSuccess: false, errorMessage };
    }
  }

  async forgotPassword(data: ForgotPasswordData): Promise<PasswordResetResponse> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.FORGOT_PASSWORD, data);
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Failed to send reset email'
      };
    }
  }

  async resetPassword(data: ResetPasswordData): Promise<PasswordResetResponse> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.RESET_PASSWORD, data);
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      return {
        success: false,
        message: axiosError.response?.data?.message || 'Failed to reset password'
      };
    }
  }

  async logout(): Promise<boolean> {
    try {
      await api.post(API_ENDPOINTS.AUTH.LOGOUT);
      return true;
    } catch (error) {
      return false;
    }
  }

  getCurrentUser(): UserInfo | null { return null; }

  async refreshToken(_refreshToken: string): Promise<AuthenticationResult> { throw new Error('Not supported'); }

  async validateToken(): Promise<boolean> { return false; }

  isAuthenticated(): boolean { return false; }

  getToken(): string | null { return null; }
}

const authService = new AuthService();

export default authService;

