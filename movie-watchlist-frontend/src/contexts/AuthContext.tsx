import React, { createContext, useContext, ReactNode, useState, useEffect } from 'react';
import authService from '../services/authService';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  UserInfo
} from '../types/auth.types';
import { STORAGE_KEYS } from '../utils/constants';

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
  validateToken: () => Promise<boolean>;
  isAuthenticated: () => boolean;
  getToken: () => string | null;
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

  // Restore user session from localStorage on mount
  useEffect(() => {
    const restoreSession = async () => {
      const storedUser = localStorage.getItem(STORAGE_KEYS.USER);
      const token = authService.getToken();
      
      if (storedUser && token) {
        try {
          const isValid = await authService.validateToken();
          if (isValid) {
            const user = JSON.parse(storedUser);
            setUser(user);
          } else {
            authService.logout();
            localStorage.removeItem(STORAGE_KEYS.USER);
          }
        } catch (error) {
          console.error('Failed to restore session:', error);
          authService.logout();
          localStorage.removeItem(STORAGE_KEYS.USER);
        }
      }
      setIsLoading(false);
    };

    restoreSession();
  }, []);

  const login = async (credentials: LoginCredentials): Promise<AuthenticationResult> => {
    const result = await authService.login(credentials);
    if (result.isSuccess && result.user) {
      setUser(result.user);
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(result.user));
    }
    return result;
  };

  const register = async (userData: RegisterData): Promise<AuthenticationResult> => {
    const result = await authService.register(userData);
    if (result.isSuccess && result.user) {
      setUser(result.user);
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(result.user));
    }
    return result;
  };

  const logout = async (): Promise<boolean> => {
    const result = await authService.logout();
    setUser(null);
    localStorage.removeItem(STORAGE_KEYS.USER);
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
    validateToken: () => authService.validateToken(),
    isAuthenticated: () => authService.isAuthenticated(),
    getToken: () => authService.getToken(),
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

