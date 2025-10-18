import React from 'react';
import { Typography, IconButton } from '@mui/material';
import MovieIcon from '@mui/icons-material/Movie';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../constants/routeConstants';

/**
 * Header Logo Component
 * 
 * Displays the app icon and title with navigation to home.
 */
const HeaderLogo: React.FC = () => {
  const navigate = useNavigate();

  const handleLogoClick = () => {
    navigate(ROUTES.HOME);
  };

  return (
    <>
      <IconButton
        size="large"
        edge="start"
        color="inherit"
        aria-label="movie watchlist home"
        sx={{ mr: 2 }}
        onClick={handleLogoClick}
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
        onClick={handleLogoClick}
      >
        MovieWatchlist
      </Typography>
    </>
  );
};

export default HeaderLogo;

