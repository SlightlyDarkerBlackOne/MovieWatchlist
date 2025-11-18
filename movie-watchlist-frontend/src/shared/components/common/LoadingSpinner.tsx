import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

interface LoadingSpinnerProps {
  message?: string;
  fullScreen?: boolean;
}

/**
 * Reusable Loading Spinner Component
 * 
 * Shows a centered loading spinner with optional message.
 * Can be used for full-screen loading or inline loading.
 */
const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({ 
  message = 'Loading...', 
  fullScreen = true 
}) => {
  return (
    <Box
      display="flex"
      flexDirection="column"
      justifyContent="center"
      alignItems="center"
      height={fullScreen ? '100vh' : 'auto'}
      gap={2}
    >
      <CircularProgress size={fullScreen ? 60 : 40} />
      {message && (
        <Typography variant="body1" color="text.secondary">
          {message}
        </Typography>
      )}
    </Box>
  );
};

export default LoadingSpinner;

