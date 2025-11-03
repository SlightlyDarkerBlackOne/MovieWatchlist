import { useState, useEffect } from 'react';
import { BrowserRouter, useNavigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline, CircularProgress, Box } from '@mui/material';
import AppRoutes from './routes/AppRoutes';
import { useAuth } from './contexts/AuthContext';
import { useError } from './contexts/ErrorContext';
import { appTheme } from './theme';
import SkipLink from './components/common/SkipLink';
import { setNavigateHandler, setGlobalErrorHandler } from './services/api';

/**
 * App Content Component
 * Handles routing logic
 */
function AppContent() {
  const { isLoading } = useAuth();
  const { showError } = useError();
  const navigate = useNavigate();
  const [resetToken, setResetToken] = useState<string | null>(null);

  useEffect(() => {
    setNavigateHandler(navigate);
    setGlobalErrorHandler(showError);
  }, [navigate, showError]);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');
    if (token) {
      setResetToken(token);
      // Clear the URL parameter for security
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, []);

  const handleResetPasswordSuccess = () => {
    setResetToken(null);
  };

  // Show loading spinner while restoring session
  if (isLoading) {
    return (
      <Box 
        sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}
        role="status"
        aria-live="polite"
        aria-label="Loading application"
      >
        <CircularProgress aria-hidden="true" />
      </Box>
    );
  }

  return (
    <AppRoutes
      resetToken={resetToken}
      onResetPasswordSuccess={handleResetPasswordSuccess}
    />
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
        <SkipLink />
        <AppContent />
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
