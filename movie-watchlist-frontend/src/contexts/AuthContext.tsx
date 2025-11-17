import React, { createContext, useContext, ReactNode, useState, useEffect } from 'react';
import {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useGetCurrentUserQuery,
} from '../store/api/authApi';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  UserInfo
} from '../types/auth.types';
import { extractErrorMessage } from '../utils/errorHandler';
import { ERROR_MESSAGES } from '../utils/constants';
 

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
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  
  const [loginMutation] = useLoginMutation();
  const [registerMutation] = useRegisterMutation();
  const [logoutMutation] = useLogoutMutation();
  const [forgotPasswordMutation] = useForgotPasswordMutation();
  const [resetPasswordMutation] = useResetPasswordMutation();
  
  const { data: currentUser, isLoading } = useGetCurrentUserQuery(undefined, {
    skip: false,
  });

  useEffect(() => {
    if (currentUser) {
      setUser(currentUser);
    } else if (!isLoading) {
      setUser(null);
    }
  }, [currentUser, isLoading]);

  const login = async (credentials: LoginCredentials): Promise<AuthenticationResult> => {
    try {
      const result = await loginMutation(credentials).unwrap();
      if (result.isSuccess && result.user) {
        setUser(result.user);
      }
      return result;
    } catch (error) {
      return {
        isSuccess: false,
        errorMessage: extractErrorMessage(error),
      };
    }
  };

  const register = async (userData: RegisterData): Promise<AuthenticationResult> => {
    try {
      const result = await registerMutation(userData).unwrap();
      if (result.isSuccess && result.user) {
        setUser(result.user);
      }
      return result;
    } catch (error) {
      return {
        isSuccess: false,
        errorMessage: extractErrorMessage(error),
      };
    }
  };

  const logout = async (): Promise<boolean> => {
    try {
      await logoutMutation().unwrap();
      setUser(null);
      return true;
    } catch {
      setUser(null);
      return false;
    }
  };

  const forgotPassword = async (data: ForgotPasswordData): Promise<PasswordResetResponse> => {
    try {
      return await forgotPasswordMutation(data).unwrap();
    } catch (error) {
      return {
        success: false,
        message: extractErrorMessage(error) || ERROR_MESSAGES.FAILED_TO_SEND_RESET_EMAIL,
      };
    }
  };

  const resetPassword = async (data: ResetPasswordData): Promise<PasswordResetResponse> => {
    try {
      return await resetPasswordMutation(data).unwrap();
    } catch (error) {
      return {
        success: false,
        message: extractErrorMessage(error) || ERROR_MESSAGES.FAILED_TO_RESET_PASSWORD,
      };
    }
  };

  const value: AuthContextType = {
    user,
    isLoading,
    login,
    register,
    logout,
    forgotPassword,
    resetPassword,
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

