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
import { Controller } from 'react-hook-form';
import ValidationService from '../../utils/validationService';
import { useAuth } from '../../contexts/AuthContext';
import { useForms } from '../../hooks/useForms';
import { resetPasswordSchema, ResetPasswordSchema } from '../../validation/schemas';
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

const ResetPasswordForm: React.FC<ResetPasswordFormProps> = ({ 
  token, 
  onSuccess, 
  onBackToLogin 
}) => {
  const { resetPassword } = useAuth();
  const [error, setError] = useState<string | null>(null);

  const form = useForms<ResetPasswordSchema>({
    schema: resetPasswordSchema,
    defaultValues: {
      newPassword: '',
      confirmPassword: ''
    }
  });

  const { control, handleSubmit, formState: { errors, isSubmitting } } = form;

  const onSubmit = handleSubmit(async (data) => {
    setError(null);

    try {
      const result = await resetPassword({
        token,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword
      });
      
      if (result.success) {
        onSuccess();
      } else {
        setError(result.message || ERROR_MESSAGES.RESET_FAILED);
      }
    } catch (error: unknown) {
      
      let errorMessage: string = ERROR_MESSAGES.UNEXPECTED_ERROR;
      if (error instanceof Error) {
        errorMessage = error.message;
      }
      
      setError(errorMessage);
    }
  });

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

      <Box component="form" onSubmit={onSubmit} noValidate>
        <Controller
          name="newPassword"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.NEW_PASSWORD}
              type="password"
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.newPassword}
              helperText={errors.newPassword?.message}
              autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
              autoFocus
              aria-label={ARIA_LABELS.NEW_PASSWORD_INPUT}
              aria-required="true"
              aria-invalid={!!errors.newPassword}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
        />
        
        <Controller
          name="confirmPassword"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.CONFIRM_PASSWORD}
              type="password"
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
              aria-label={ARIA_LABELS.CONFIRM_PASSWORD_INPUT}
              aria-required="true"
              aria-invalid={!!errors.confirmPassword}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
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
          disabled={isSubmitting}
          aria-label={isSubmitting ? ARIA_LABELS.RESET_PASSWORD_BUTTON_LOADING : ARIA_LABELS.RESET_PASSWORD_BUTTON}
        >
          {isSubmitting ? (
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
            disabled={isSubmitting}
            sx={{ 
              textDecoration: 'none',
              '&:hover': { textDecoration: 'underline' },
              cursor: isSubmitting ? 'not-allowed' : 'pointer',
              opacity: isSubmitting ? 0.5 : 1
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

