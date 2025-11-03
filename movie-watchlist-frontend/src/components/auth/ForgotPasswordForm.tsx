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
import { useAuth } from '../../contexts/AuthContext';
import { useForms } from '../../hooks/useForms';
import { forgotPasswordSchema, ForgotPasswordSchema } from '../../validation/schemas';
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
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const form = useForms<ForgotPasswordSchema>({
    schema: forgotPasswordSchema,
    defaultValues: {
      email: ''
    }
  });

  const { control, handleSubmit, formState: { errors, isSubmitting }, reset } = form;

  const onSubmit = handleSubmit(async (data) => {
    setError(null);
    setSuccess(false);

    try {
      const result = await forgotPassword({ email: data.email });
      
      if (result.success) {
        setSuccess(true);
        reset();
      } else {
        setError(result.message || ERROR_MESSAGES.SEND_RESET_FAILED);
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

      <Box component="form" onSubmit={onSubmit} noValidate>
        <Controller
          name="email"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.EMAIL}
              type="email"
              margin="normal"
              required
              disabled={isSubmitting || success}
              error={!!errors.email}
              helperText={errors.email?.message}
              autoComplete={AUTOCOMPLETE_VALUES.EMAIL}
              autoFocus
              aria-label={ARIA_LABELS.EMAIL_INPUT}
              aria-required="true"
              aria-invalid={!!errors.email}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
        />
        
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: FORM_SETTINGS.MARGIN_BOTTOM }}
          disabled={isSubmitting || success}
          aria-label={isSubmitting ? ARIA_LABELS.SEND_RESET_LINK_BUTTON_LOADING : ARIA_LABELS.SEND_RESET_LINK_BUTTON}
        >
          {isSubmitting ? (
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

export default ForgotPasswordForm;

