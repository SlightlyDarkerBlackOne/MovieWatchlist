import React from 'react';
import { Box, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { ROUTES } from '../../constants/routeConstants';

/**
 * Header Auth Buttons Component
 * 
 * Displays authentication-related buttons:
 * - My Watchlist (always visible)
 * - Login (when not authenticated)
 * - Logout (when authenticated)
 */
const HeaderAuthButtons: React.FC = () => {
  const navigate = useNavigate();
  const { logout, isAuthenticated } = useAuth();

  const handleLogout = async () => {
    await logout();
    // Force a full page reload to clear all state
    window.location.href = ROUTES.LOGIN;
  };

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
      <Button 
        color="inherit" 
        onClick={() => navigate(ROUTES.WATCHLIST)}
      >
        My Watchlist
      </Button>
      {isAuthenticated() ? (
        <Button color="inherit" onClick={handleLogout}>
          Logout
        </Button>
      ) : (
        <Button color="inherit" onClick={() => navigate(ROUTES.LOGIN)}>
          Login
        </Button>
      )}
    </Box>
  );
};

export default HeaderAuthButtons;

