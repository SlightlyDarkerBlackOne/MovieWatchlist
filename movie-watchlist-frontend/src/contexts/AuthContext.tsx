import React, { createContext, useContext, ReactNode, useState, useEffect } from 'react';
import authService from '../services/authService';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  User
} from '../types/auth.types';
import { STORAGE_KEYS } from '../utils/constants';

/**
 * Auth Context Interface
 * Defines all authentication-related methods available to components
 */
export interface AuthContextType {
  user: User | null;
  login: (credentials: LoginCredentials) => Promise<AuthenticationResult>;
  register: (userData: RegisterData) => Promise<AuthenticationResult>;
  logout: () => Promise<boolean>;
  forgotPassword: (data: ForgotPasswordData) => Promise<PasswordResetResponse>;
  resetPassword: (data: ResetPasswordData) => Promise<PasswordResetResponse>;
  validateToken: () => Promise<boolean>;
  isAuthenticated: () => boolean;
  getToken: () => string | null;
}

// Create the context with undefined default (will be provided by AuthProvider)
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
  const [user, setUser] = useState<User | null>(null);

  // Load user from localStorage on mount
  useEffect(() => {
    const storedUser = localStorage.getItem(STORAGE_KEYS.USER);
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch (error) {
        console.error('Failed to parse stored user:', error);
        localStorage.removeItem(STORAGE_KEYS.USER);
      }
    }
  }, []);

  // Wrap authService methods to provide them through context
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

