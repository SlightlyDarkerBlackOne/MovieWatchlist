import React from 'react';
import { Snackbar, Alert } from '@mui/material';

interface SuccessToastProps {
  message: string | null;
  onClose: () => void;
}

const SuccessToast: React.FC<SuccessToastProps> = ({ message, onClose }) => {
  if (!message) return null;

  return (
    <Snackbar
      open={!!message}
      autoHideDuration={3000}
      onClose={onClose}
      anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
    >
      <Alert severity="success" variant="filled" sx={{ width: '100%' }} onClose={onClose}>
        {message}
      </Alert>
    </Snackbar>
  );
};

export default SuccessToast;

