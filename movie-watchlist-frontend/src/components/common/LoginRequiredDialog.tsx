import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../../constants/routeConstants';

interface LoginRequiredDialogProps {
  open: boolean;
  onClose: () => void;
}

/**
 * Login Required Dialog Component
 * 
 * Reusable dialog that prompts users to log in when they attempt
 * to perform an action that requires authentication.
 */
const LoginRequiredDialog: React.FC<LoginRequiredDialogProps> = ({ open, onClose }) => {
  const navigate = useNavigate();

  const handleGoToLogin = () => {
    onClose();
    navigate(ROUTES.LOGIN);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Login Required</DialogTitle>
      <DialogContent>
        <Typography variant="body1" sx={{ pt: 2 }}>
          Please log in to add movies to your watchlist.
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button 
          onClick={handleGoToLogin} 
          variant="contained" 
          color="primary"
        >
          Go to Login
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default LoginRequiredDialog;

