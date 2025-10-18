import { useState, useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider, CssBaseline, CircularProgress, Box } from '@mui/material';
import AppRoutes from './routes/AppRoutes';
import { WatchlistProvider } from './contexts/WatchlistContext';
import { useAuth } from './contexts/AuthContext';
import { appTheme } from './theme';

/**
 * App Content Component
 * Handles routing logic
 */
function AppContent() {
  const { isLoading } = useAuth();
  const [resetToken, setResetToken] = useState<string | null>(null);

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
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <WatchlistProvider>
      <AppRoutes
        resetToken={resetToken}
        onResetPasswordSuccess={handleResetPasswordSuccess}
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
