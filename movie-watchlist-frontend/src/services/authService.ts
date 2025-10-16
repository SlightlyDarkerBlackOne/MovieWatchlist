import api from './api';
import { API_ENDPOINTS, STORAGE_KEYS } from '../utils/constants';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  User
} from '../types/auth.types';
import { AxiosError } from '../types/error.types';

class AuthService {
  async login(credentials: LoginCredentials): Promise<AuthenticationResult> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.LOGIN, credentials);
      const result = response.data;
      
      if (result.isSuccess) {
        // Store tokens in localStorage
        // NOTE: For MVP, localStorage is acceptable. In production, consider:
        // - httpOnly cookies (prevents XSS but requires backend changes)
        // - In-memory storage with refresh token rotation
        // For this portfolio project, localStorage demonstrates understanding of token management.
        localStorage.setItem(STORAGE_KEYS.TOKEN, result.token);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, result.refreshToken);
      }
      
      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      return {
        isSuccess: false,
        errorMessage: axiosError.response?.data?.message || 'Login failed'
      };
    }
  }

  async register(userData: RegisterData): Promise<AuthenticationResult> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.REGISTER, userData);
      const result = response.data;
      
      if (result.isSuccess) {
        // Store tokens in localStorage
        localStorage.setItem(STORAGE_KEYS.TOKEN, result.token);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, result.refreshToken);
      }
      
      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      // Try to extract more specific error message
      let errorMessage = 'Registration failed';
      if (axiosError.response?.data?.message) {
        errorMessage = axiosError.response.data.message;
      } else if (axiosError.response?.data?.errors) {
        // Handle validation errors
        const errors = axiosError.response.data.errors;
        const errorMessages = Object.values(errors).flat();
        errorMessage = errorMessages.join(', ');
      } else if (axiosError.message) {
        errorMessage = axiosError.message;
      }
      
      return {
        isSuccess: false,
        errorMessage
      };
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
      console.error('Logout error:', error);
      return false;
    } finally {
      // Always clear local storage
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
    }
  }

  getCurrentUser(): User | null {
    try {
      const userStr = localStorage.getItem(STORAGE_KEYS.USER);
      if (!userStr) return null;
      return JSON.parse(userStr);
    } catch (error) {
      console.error('Error getting current user:', error);
      return null;
    }
  }

  async refreshToken(refreshToken: string): Promise<AuthenticationResult> {
    try {
      const response = await api.post(API_ENDPOINTS.AUTH.REFRESH, { refreshToken });
      const result = response.data;
      
      // Store new tokens
      if (result.token) {
        localStorage.setItem(STORAGE_KEYS.TOKEN, result.token);
      }
      if (result.refreshToken) {
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, result.refreshToken);
      }
      if (result.user) {
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(result.user));
      }
      
      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Refresh token error:', axiosError);
      // Clear tokens if refresh fails
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
      throw new Error(axiosError.response?.data?.message || 'Failed to refresh token');
    }
  }

  async validateToken(): Promise<boolean> {
    // Client-side JWT validation - no API call needed!
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (!token) {
      return false;
    }

    try {
      // Decode JWT to check expiration without making an API call
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expirationTime = payload.exp * 1000; // Convert to milliseconds
      const now = Date.now();
      
      // Check if token is expired or about to expire (within 5 minutes)
      const bufferTime = 5 * 60 * 1000; // 5 minutes
      if (now >= (expirationTime - bufferTime)) {
        // Token expired or about to expire - try to refresh
        const storedRefreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        if (storedRefreshToken) {
          try {
            await this.refreshToken(storedRefreshToken);
            return true; // Successfully refreshed
          } catch {
            // Refresh failed - clear storage
            localStorage.removeItem(STORAGE_KEYS.TOKEN);
            localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
            localStorage.removeItem(STORAGE_KEYS.USER);
            return false;
          }
        }
        // No refresh token - clear storage
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
        return false;
      }
      
      return true; // Token is valid
    } catch (error) {
      console.error('Token validation error:', error);
      // If we can't decode the token, it's invalid
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
      return false;
    }
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    return !!token;
  }

  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.TOKEN);
  }
}

export default new AuthService();
