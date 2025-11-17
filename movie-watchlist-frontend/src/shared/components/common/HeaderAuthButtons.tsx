import React from 'react';
import { Box, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../features/auth/contexts/AuthContext';
import { ROUTES } from '../../constants/routeConstants';
import { HEADER_AUTH_BUTTON_TEXT } from '../../constants/appConstants';

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
    navigate(ROUTES.MOVIES);
  };

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
      <Button 
        color="inherit" 
        onClick={() => navigate(ROUTES.WATCHLIST)}
      >
        {HEADER_AUTH_BUTTON_TEXT.MY_WATCHLIST}
      </Button>
      {isAuthenticated() ? (
        <Button color="inherit" onClick={handleLogout}>
          {HEADER_AUTH_BUTTON_TEXT.LOGOUT}
        </Button>
      ) : (
        <Button color="inherit" onClick={() => navigate(ROUTES.LOGIN)}>
          {HEADER_AUTH_BUTTON_TEXT.LOGIN}
        </Button>
      )}
    </Box>
  );
};

export default HeaderAuthButtons;

