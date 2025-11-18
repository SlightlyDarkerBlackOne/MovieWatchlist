import React, { createContext, useContext, ReactNode } from 'react';
import {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useGetCurrentUserQuery,
} from '../api/authApi';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthenticationResult,
  ForgotPasswordData,
  ResetPasswordData,
  PasswordResetResponse,
  UserInfo,
} from '../model/auth.types';
import { getErrorMessage } from '../../../shared/lib/errorHandler';
import { ERROR_MESSAGES } from '../../../shared/constants/appConstants';

/**
 * Auth Context Interface
 * Thin wrapper around RTK Query - provides convenient API without duplicating state.
 * Server state (user data) is managed entirely by RTK Query.
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
 * Thin wrapper around RTK Query that provides authentication methods.
 * Does NOT duplicate state - user data comes directly from RTK Query cache.
 */
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [loginMutation] = useLoginMutation();
  const [registerMutation] = useRegisterMutation();
  const [logoutMutation] = useLogoutMutation();
  const [forgotPasswordMutation] = useForgotPasswordMutation();
  const [resetPasswordMutation] = useResetPasswordMutation();
  
  const { data: currentUser, isLoading } = useGetCurrentUserQuery(undefined, {
    skip: false,
  });

  const login = async (credentials: LoginCredentials): Promise<AuthenticationResult> => {
    try {
      return await loginMutation(credentials).unwrap();
    } catch (error) {
      return {
        isSuccess: false,
        errorMessage: getErrorMessage(error),
      };
    }
  };

  const register = async (userData: RegisterData): Promise<AuthenticationResult> => {
    try {
      return await registerMutation(userData).unwrap();
    } catch (error) {
      return {
        isSuccess: false,
        errorMessage: getErrorMessage(error),
      };
    }
  };

  const logout = async (): Promise<boolean> => {
    try {
      await logoutMutation().unwrap();
      return true;
    } catch {
      return false;
    }
  };

  const forgotPassword = async (data: ForgotPasswordData): Promise<PasswordResetResponse> => {
    try {
      return await forgotPasswordMutation(data).unwrap();
    } catch (error) {
      return {
        success: false,
        message: getErrorMessage(error) || ERROR_MESSAGES.FAILED_TO_SEND_RESET_EMAIL,
      };
    }
  };

  const resetPassword = async (data: ResetPasswordData): Promise<PasswordResetResponse> => {
    try {
      return await resetPasswordMutation(data).unwrap();
    } catch (error) {
      return {
        success: false,
        message: getErrorMessage(error) || ERROR_MESSAGES.FAILED_TO_RESET_PASSWORD,
      };
    }
  };

  const value: AuthContextType = {
    user: currentUser ?? null,
    isLoading,
    login,
    register,
    logout,
    forgotPassword,
    resetPassword,
    isAuthenticated: () => !!currentUser,
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

