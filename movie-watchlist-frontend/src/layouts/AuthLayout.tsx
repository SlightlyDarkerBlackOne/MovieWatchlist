import React from 'react';
import { Alert, Box } from '@mui/material';
import Header from '../components/common/Header';
import LoginForm from '../components/auth/LoginForm';
import RegisterForm from '../components/auth/RegisterForm';
import ForgotPasswordForm from '../components/auth/ForgotPasswordForm';
import ResetPasswordForm from '../components/auth/ResetPasswordForm';
import { AUTH_VIEWS, type AuthView } from '../constants/routeConstants';

interface AuthLayoutProps {
  view: AuthView;
  resetToken?: string | null;
  successMessage?: string | null;
  onLoginSuccess: () => void;
  onRegister: () => void;
  onForgotPassword: () => void;
  onBackToLogin: () => void;
  onResetPasswordSuccess: () => void;
}

/**
 * Auth Layout Component
 * 
 * Handles rendering of all authentication-related views.
 * Centralizes the auth view logic and reduces complexity in App.tsx.
 */
const AuthLayout: React.FC<AuthLayoutProps> = ({
  view,
  resetToken,
  successMessage,
  onLoginSuccess,
  onRegister,
  onForgotPassword,
  onBackToLogin,
  onResetPasswordSuccess,
}) => {
  return (
    <Box>
      <Header showAuth={false} />
      <Box sx={{ mt: 4 }}>
        {/* Show success message with login form */}
        {successMessage && (
          <Alert severity="success" sx={{ maxWidth: 400, mx: 'auto', mb: 2 }}>
            {successMessage}
          </Alert>
        )}

        {/* Render appropriate view based on current state */}
        {(() => {
          switch (view) {
            case AUTH_VIEWS.REGISTER:
              return (
                <RegisterForm 
                  onRegisterSuccess={onLoginSuccess}
                  onBackToLogin={onBackToLogin}
                />
              );

            case AUTH_VIEWS.FORGOT_PASSWORD:
              return <ForgotPasswordForm onBackToLogin={onBackToLogin} />;
              
            case AUTH_VIEWS.RESET_PASSWORD:
              return resetToken ? (
                <ResetPasswordForm 
                  token={resetToken} 
                  onSuccess={onResetPasswordSuccess}
                  onBackToLogin={onBackToLogin}
                />
              ) : (
                <LoginForm 
                  onLoginSuccess={onLoginSuccess} 
                  onRegister={onRegister}
                  onForgotPassword={onForgotPassword} 
                />
              );
              
            case AUTH_VIEWS.LOGIN:
            default:
              return (
                <LoginForm 
                  onLoginSuccess={onLoginSuccess}
                  onRegister={onRegister}
                  onForgotPassword={onForgotPassword} 
                />
              );
          }
        })()}
      </Box>
    </Box>
  );
};

export default AuthLayout;

