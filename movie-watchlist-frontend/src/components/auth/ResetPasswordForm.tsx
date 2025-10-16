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
  ARIA_LABELS,
  AUTOCOMPLETE_VALUES,
  FORM_SETTINGS
} from '../../constants/formConstants';

interface ResetPasswordFormProps {
  token: string;
  onSuccess: () => void;
  onBackToLogin: () => void;
}

interface FormErrors {
  newPassword?: string;
  confirmPassword?: string;
}

const ResetPasswordForm: React.FC<ResetPasswordFormProps> = ({ 
  token, 
  onSuccess, 
  onBackToLogin 
}) => {
  const { resetPassword } = useAuth();
  const [passwords, setPasswords] = useState({
    newPassword: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setPasswords(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear field error when user starts typing
    if (fieldErrors[name as keyof FormErrors]) {
      setFieldErrors(prev => ({
        ...prev,
        [name]: undefined
      }));
    }
    
    // Clear general error when user starts typing
    if (error) {
      setError(null);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};
    
    // Validate new password with full strength requirements
    const passwordValidation = ValidationService.validatePasswordStrength(
      passwords.newPassword
    );
    if (!passwordValidation.isValid) {
      newErrors.newPassword = passwordValidation.error;
    }
    
    // Validate confirm password
    if (!passwords.confirmPassword) {
      newErrors.confirmPassword = ERROR_MESSAGES.CONFIRM_PASSWORD_REQUIRED;
    } else if (passwords.newPassword !== passwords.confirmPassword) {
      newErrors.confirmPassword = ERROR_MESSAGES.PASSWORDS_DO_NOT_MATCH;
    }
    
    setFieldErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate form before submission
    if (!validateForm()) {
      return;
    }
    
    setLoading(true);
    setError(null);

    try {
      const result = await resetPassword({
        token,
        newPassword: passwords.newPassword,
        confirmPassword: passwords.confirmPassword
      });
      
      if (result.success) {
        onSuccess();
      } else {
        setError(result.message || ERROR_MESSAGES.RESET_FAILED);
      }
    } catch (error: unknown) {
      console.error('Reset password error:', error);
      
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
        {FORM_TITLES.RESET_PASSWORD}
      </Typography>
      
      <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 3 }}>
        {FORM_DESCRIPTIONS.RESET_PASSWORD}
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

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <TextField
          fullWidth
          label={FORM_LABELS.NEW_PASSWORD}
          name="newPassword"
          type="password"
          value={passwords.newPassword}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.newPassword}
          helperText={fieldErrors.newPassword}
          autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
          autoFocus
          aria-label={ARIA_LABELS.NEW_PASSWORD_INPUT}
          aria-required="true"
          aria-invalid={!!fieldErrors.newPassword}
        />
        
        <TextField
          fullWidth
          label={FORM_LABELS.CONFIRM_PASSWORD}
          name="confirmPassword"
          type="password"
          value={passwords.confirmPassword}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.confirmPassword}
          helperText={fieldErrors.confirmPassword}
          autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
          aria-label={ARIA_LABELS.CONFIRM_PASSWORD_INPUT}
          aria-required="true"
          aria-invalid={!!fieldErrors.confirmPassword}
        />

        <Alert severity="info" sx={{ mt: 2, mb: 2 }}>
          <Typography variant="caption">
            Password must contain {ValidationService.getPasswordRequirementsText()}
          </Typography>
        </Alert>
        
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 1, mb: FORM_SETTINGS.MARGIN_BOTTOM }}
          disabled={loading}
          aria-label={loading ? ARIA_LABELS.RESET_PASSWORD_BUTTON_LOADING : ARIA_LABELS.RESET_PASSWORD_BUTTON}
        >
          {loading ? (
            <>
              <CircularProgress size={FORM_SETTINGS.LOADING_SPINNER_SIZE} sx={{ mr: 1 }} color="inherit" />
              {FORM_LABELS.RESETTING}
            </>
          ) : (
            FORM_LABELS.RESET_PASSWORD
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

export default ResetPasswordForm;

