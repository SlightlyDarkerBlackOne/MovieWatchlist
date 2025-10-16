import React from 'react';
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { AuthenticatedLayout } from '../components/layout';
import { LoginForm, RegisterForm, ForgotPasswordForm, ResetPasswordForm } from '../components/auth';
import MoviesPage from '../pages/MoviesPage';
import MovieDetailsPage from '../pages/MovieDetailsPage';
import WatchlistPage from '../pages/WatchlistPage';
import { ROUTES } from '../constants/routeConstants';

interface AppRoutesProps {
  isAuthenticated: boolean;
  onLoginSuccess: () => void;
  onRegister: () => void;
  onForgotPassword: () => void;
  onBackToLogin: () => void;
  onResetPasswordSuccess: () => void;
  onLogout: () => void;
  resetToken?: string | null;
}

/**
 * Application Routes Component
 * 
 * Defines all routes and handles route-based navigation.
 * Separates routing logic from App.tsx for better organization.
 */
const AppRoutes: React.FC<AppRoutesProps> = ({
  isAuthenticated,
  onLoginSuccess,
  onRegister,
  onForgotPassword,
  onBackToLogin,
  onResetPasswordSuccess,
  onLogout,
  resetToken,
}) => {
  const navigate = useNavigate();

  return (
    <>
      <Routes>
        {/* Public Routes */}
        <Route 
          path={ROUTES.LOGIN} 
          element={
            isAuthenticated ? (
              <Navigate to={ROUTES.MOVIES} replace />
            ) : (
              <LoginForm 
                onLoginSuccess={onLoginSuccess} 
                onRegister={() => navigate(ROUTES.REGISTER)}
                onForgotPassword={() => navigate(ROUTES.FORGOT_PASSWORD)} 
              />
            )
          } 
        />
        
        <Route 
          path={ROUTES.REGISTER} 
          element={
            isAuthenticated ? (
              <Navigate to={ROUTES.MOVIES} replace />
            ) : (
              <RegisterForm 
                onRegisterSuccess={onLoginSuccess}
                onBackToLogin={() => navigate(ROUTES.LOGIN)}
              />
            )
          } 
        />
        
        <Route 
          path={ROUTES.FORGOT_PASSWORD} 
          element={
            isAuthenticated ? (
              <Navigate to={ROUTES.MOVIES} replace />
            ) : (
              <ForgotPasswordForm onBackToLogin={() => navigate(ROUTES.LOGIN)} />
            )
          } 
        />
        
        <Route 
          path={ROUTES.RESET_PASSWORD} 
          element={
            isAuthenticated ? (
              <Navigate to={ROUTES.MOVIES} replace />
            ) : resetToken ? (
              <ResetPasswordForm 
                token={resetToken} 
                onSuccess={onResetPasswordSuccess}
                onBackToLogin={() => navigate(ROUTES.LOGIN)}
              />
            ) : (
              <Navigate to={ROUTES.LOGIN} replace />
            )
          } 
        />

        {/* Protected Routes */}

        <Route 
          path={ROUTES.MOVIES} 
          element={
            isAuthenticated ? (
              <AuthenticatedLayout showSearch={true}>
                <MoviesPage />
              </AuthenticatedLayout>
            ) : (
              <Navigate to={ROUTES.LOGIN} replace />
            )
          } 
        />

        <Route 
          path="/movies/:tmdbId" 
          element={
            isAuthenticated ? (
              <AuthenticatedLayout showSearch={true}>
                <MovieDetailsPage />
              </AuthenticatedLayout>
            ) : (
              <Navigate to={ROUTES.LOGIN} replace />
            )
          } 
        />

        <Route 
          path={ROUTES.WATCHLIST} 
          element={
            isAuthenticated ? (
              <AuthenticatedLayout showSearch={true}>
                <WatchlistPage />
              </AuthenticatedLayout>
            ) : (
              <Navigate to={ROUTES.LOGIN} replace />
            )
          } 
        />

        {/* Default Route - Movies Page is now the home page */}
        <Route 
          path={ROUTES.HOME} 
          element={
            <Navigate to={isAuthenticated ? ROUTES.MOVIES : ROUTES.LOGIN} replace />
          } 
        />

        {/* Catch all - redirect to home */}
        <Route path="*" element={<Navigate to={ROUTES.HOME} replace />} />
      </Routes>
    </>
  );
};

export default AppRoutes;

