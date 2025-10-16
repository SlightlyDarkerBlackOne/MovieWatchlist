import React, { useState } from 'react';
import { 
  Button, 
  TextField, 
  Box, 
  Typography, 
  Alert, 
  Paper,
  CircularProgress,
  Link
} from '@mui/material';
import ValidationService from '../../utils/validationService';
import { useAuth } from '../../contexts/AuthContext';
import {
  FORM_LABELS,
  FORM_TITLES,
  FORM_DESCRIPTIONS,
  ERROR_MESSAGES,
  SUCCESS_MESSAGES,
  ARIA_LABELS,
  AUTOCOMPLETE_VALUES,
  FORM_SETTINGS
} from '../../constants/formConstants';

interface ForgotPasswordFormProps {
  onBackToLogin: () => void;
}

const ForgotPasswordForm: React.FC<ForgotPasswordFormProps> = ({ onBackToLogin }) => {
  const { forgotPassword } = useAuth();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [fieldError, setFieldError] = useState<string | undefined>();

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value);
    
    // Clear errors when user starts typing
    if (fieldError) {
      setFieldError(undefined);
    }
    if (error) {
      setError(null);
    }
  };

  const validateForm = (): boolean => {
    const emailValidation = ValidationService.validateEmailField(email);
    
    if (!emailValidation.isValid) {
      setFieldError(emailValidation.error);
      return false;
    }
    
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate form before submission
    if (!validateForm()) {
      return;
    }
    
    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const result = await forgotPassword({ email });
      
      if (result.success) {
        setSuccess(true);
        setEmail(''); // Clear the email field
      } else {
        setError(result.message || ERROR_MESSAGES.SEND_RESET_FAILED);
      }
    } catch (error: unknown) {
      console.error('Forgot password error:', error);
      
      let errorMessage: string = ERROR_MESSAGES.UNEXPECTED_ERROR;
      if (error instanceof Error) {
        errorMessage = error.message;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper elevation={3} sx={{ p: FORM_SETTINGS.PAPER_PADDING, maxWidth: FORM_SETTINGS.MAX_FORM_WIDTH, mx: 'auto', mt: FORM_SETTINGS.MARGIN_TOP }}>
      <Typography variant="h4" component="h1" gutterBottom align="center">
        {FORM_TITLES.FORGOT_PASSWORD}
      </Typography>
      
      <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 3 }}>
        {FORM_DESCRIPTIONS.FORGOT_PASSWORD}
      </Typography>
      
      {error && (
        <Alert 
          severity="error" 
          sx={{ mb: 2 }}
          role="alert"
          aria-live="polite"
        >
          {error}
        </Alert>
      )}

      {success && (
        <Alert 
          severity="success" 
          sx={{ mb: 2 }}
          role="alert"
          aria-live="polite"
        >
          {SUCCESS_MESSAGES.PASSWORD_RESET_EMAIL_SENT}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <TextField
          fullWidth
          label={FORM_LABELS.EMAIL}
          name="email"
          type="email"
          value={email}
          onChange={handleEmailChange}
          margin="normal"
          required
          disabled={loading || success}
          error={!!fieldError}
          helperText={fieldError}
          autoComplete={AUTOCOMPLETE_VALUES.EMAIL}
          autoFocus
          aria-label={ARIA_LABELS.EMAIL_INPUT}
          aria-required="true"
          aria-invalid={!!fieldError}
        />
        
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: FORM_SETTINGS.MARGIN_BOTTOM }}
          disabled={loading || success}
          aria-label={loading ? ARIA_LABELS.SEND_RESET_LINK_BUTTON_LOADING : ARIA_LABELS.SEND_RESET_LINK_BUTTON}
        >
          {loading ? (
            <>
              <CircularProgress size={FORM_SETTINGS.LOADING_SPINNER_SIZE} sx={{ mr: 1 }} color="inherit" />
              {FORM_LABELS.SENDING}
            </>
          ) : (
            FORM_LABELS.SEND_RESET_LINK
          )}
        </Button>

        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
          <Link
            component="button"
            type="button"
            variant="body2"
            onClick={onBackToLogin}
            disabled={loading}
            sx={{ 
              textDecoration: 'none',
              '&:hover': { textDecoration: 'underline' },
              cursor: loading ? 'not-allowed' : 'pointer',
              opacity: loading ? 0.5 : 1
            }}
          >
            {FORM_LABELS.BACK_TO_LOGIN}
          </Link>
        </Box>
      </Box>
    </Paper>
  );
};

export default ForgotPasswordForm;

