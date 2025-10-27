import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import { ErrorOutline } from '@mui/icons-material';

interface ErrorStateProps {
  message?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

const ErrorState: React.FC<ErrorStateProps> = ({ 
  message = 'Something went wrong', 
  onRetry,
  retryLabel = 'Try Again'
}) => {
  return (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      py={8}
      px={2}
    >
      <ErrorOutline sx={{ fontSize: 64, color: 'error.main', mb: 2 }} />
      <Typography variant="h6" color="text.secondary" align="center" gutterBottom>
        {message}
      </Typography>
      {onRetry && (
        <Button variant="contained" color="primary" onClick={onRetry} sx={{ mt: 2 }}>
          {retryLabel}
        </Button>
      )}
    </Box>
  );
};

export default ErrorState;


