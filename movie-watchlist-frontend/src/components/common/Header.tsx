import React from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Box, 
  IconButton, 
  Button
} from '@mui/material';
import MovieIcon from '@mui/icons-material/Movie';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../constants/routeConstants';
import { useAuth } from '../../contexts/AuthContext';
import SearchDropdown from './SearchDropdown';

interface HeaderProps {
  showAuth?: boolean;
  onSearch?: (query: string) => void;
  showSearch?: boolean;
}

const Header: React.FC<HeaderProps> = ({ showAuth = false, onSearch, showSearch = false }) => {
  const navigate = useNavigate();
  const { logout } = useAuth();

  const handleLogout = async () => {
    await logout();
    // Force a full page reload to clear all state
    window.location.href = ROUTES.LOGIN;
  };

  const handleFullSearch = (query: string) => {
    if (onSearch) {
      onSearch(query);
    }
  };

  return (
    <AppBar position="static" elevation={2}>
      <Toolbar>
        {/* Left Section: Logo */}
        <IconButton
          size="large"
          edge="start"
          color="inherit"
          aria-label="movie watchlist home"
          sx={{ mr: 2 }}
          onClick={() => navigate(ROUTES.HOME)}
        >
          <MovieIcon />
        </IconButton>
        
        <Typography 
          variant="h6" 
          component="div" 
          sx={{ 
            cursor: 'pointer',
            fontWeight: 600,
            mr: 3
          }}
          onClick={() => navigate(ROUTES.HOME)}
        >
          MovieWatchlist
        </Typography>

        {/* Search Bar - Close to Left */}
        {showSearch && (
          <Box sx={{ width: '100%', maxWidth: 600, mr: 3, ml: { xs: 2, sm: 4, md: 8 } }}>
            <SearchDropdown onFullSearch={handleFullSearch} />
          </Box>
        )}

        {/* Spacer to push buttons to the right */}
        <Box sx={{ flexGrow: 1 }} />

        {/* Right Section: Auth Buttons */}
        {showAuth && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button 
              color="inherit" 
              onClick={() => navigate(ROUTES.WATCHLIST)}
            >
              My Watchlist
            </Button>
            <Button color="inherit" onClick={handleLogout}>
              Logout
            </Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default Header;

