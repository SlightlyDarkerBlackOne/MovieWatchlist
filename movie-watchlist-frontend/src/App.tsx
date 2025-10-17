import { useState, useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import AppRoutes from './routes/AppRoutes';
import { useAuth } from './contexts/AuthContext';
import { WatchlistProvider } from './contexts/WatchlistContext';
import { appTheme } from './theme';

/**
 * App Content Component
 * Handles auth state and routing logic
 */
function AppContent() {
  const { isAuthenticated: checkIsAuthenticated, validateToken } = useAuth();
  const [isAuthenticated, setIsAuthenticated] = useState(checkIsAuthenticated());
  const [resetToken, setResetToken] = useState<string | null>(null);

  // Validate token in background for logged-in users
  useEffect(() => {
    let cancelled = false;

    const validateAuth = async () => {
      if (checkIsAuthenticated() && !cancelled) {
        const isValid = await validateToken();
        setIsAuthenticated(isValid);
      }
    };

    validateAuth();

    return () => {
      cancelled = true;
    };
  }, [checkIsAuthenticated, validateToken]);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');
    if (token) {
      setResetToken(token);
      // Clear the URL parameter for security
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, []);

  const handleLoginSuccess = () => {
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
  };

  const handleRegister = () => {
    // Navigation handled by React Router
  };

  const handleForgotPassword = () => {
    // Navigation handled by React Router
  };

  const handleBackToLogin = () => {
    // Navigation handled by React Router
  };

  const handleResetPasswordSuccess = () => {
    setResetToken(null);
  };

  return (
    <WatchlistProvider>
      <AppRoutes
        isAuthenticated={isAuthenticated}
        onLoginSuccess={handleLoginSuccess}
        onRegister={handleRegister}
        onForgotPassword={handleForgotPassword}
        onBackToLogin={handleBackToLogin}
        onResetPasswordSuccess={handleResetPasswordSuccess}
        onLogout={handleLogout}
        resetToken={resetToken}
      />
    </WatchlistProvider>
  );
}

/**
 * Main App Component
 * 
 * Clean composition of providers and routing.
 * All business logic extracted to AppContent.
 */
function App() {
  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <BrowserRouter>
        <AppContent />
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
