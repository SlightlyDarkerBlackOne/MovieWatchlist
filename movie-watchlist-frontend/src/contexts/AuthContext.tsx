import React, { createContext, useContext, ReactNode, useState, useEffect } from 'react';
import authService from '../services/authService';
import api from '../services/api';
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
 

/**
 * Auth Context Interface
 * Defines all authentication-related methods available to components
 */
export interface AuthContextType {
  user: UserInfo | null;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<AuthenticationResult>;
  register: (userData: RegisterData) => Promise<AuthenticationResult>;
  logout: () => Promise<boolean>;
  forgotPassword: (data: ForgotPasswordData) => Promise<PasswordResetResponse>;
  resetPassword: (data: ResetPasswordData) => Promise<PasswordResetResponse>;
  isAuthenticated: () => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Auth Provider Component
 * Wraps the app and provides authentication methods to all children
 * 
 * C# Equivalent: Similar to DI Container or Service Provider
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore user session via /Auth/me on mount
  useEffect(() => {
    const restoreSession = async () => {
      try {
        const response = await api.get(API_ENDPOINTS.AUTH.ME);
        setUser(response.data);
      } catch {
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    restoreSession();
  }, []);

  const login = async (credentials: LoginCredentials): Promise<AuthenticationResult> => {
    const result = await authService.login(credentials);
    if (result.isSuccess && result.user) {
      setUser(result.user);
    }
    return result;
  };

  const register = async (userData: RegisterData): Promise<AuthenticationResult> => {
    const result = await authService.register(userData);
    if (result.isSuccess && result.user) {
      setUser(result.user);
    }
    return result;
  };

  const logout = async (): Promise<boolean> => {
    const result = await authService.logout();
    setUser(null);
    return result;
  };

  const value: AuthContextType = {
    user,
    isLoading,
    login,
    register,
    logout,
    forgotPassword: (data) => authService.forgotPassword(data),
    resetPassword: (data) => authService.resetPassword(data),
    isAuthenticated: () => !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

/**
 * Custom Hook: useAuth
 * Provides access to authentication methods
 * 
 * Usage in components:
 * const { login, logout, isAuthenticated } = useAuth();
 * 
 * C# Equivalent: Like injecting IAuthenticationService via constructor
 */
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
};

export default AuthContext;

