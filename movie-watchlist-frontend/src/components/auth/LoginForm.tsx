import React, { useState } from 'react';
import { 
  Button, 
  TextField, 
  Box, 
  Typography, 
  Alert, 
  Paper,
  CircularProgress,
  Link,
  IconButton,
  InputAdornment
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { Controller } from 'react-hook-form';
import Header from '../common/Header';
import { useAuth } from '../../contexts/AuthContext';
import { useForms } from '../../hooks/useForms';
import { loginSchema, LoginSchema } from '../../validation/schemas';
import { 
  FORM_LABELS, 
  FORM_TITLES, 
  ERROR_MESSAGES, 
  ARIA_LABELS, 
  AUTOCOMPLETE_VALUES,
  FORM_SETTINGS,
  TEST_CREDENTIALS
} from '../../constants/formConstants';

interface LoginFormProps {
  onLoginSuccess: () => void;
  onForgotPassword: () => void;
  onRegister: () => void;
}

const LoginForm: React.FC<LoginFormProps> = ({ onLoginSuccess, onForgotPassword, onRegister }) => {
  const { login } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);

  const form = useForms<LoginSchema>({
    schema: loginSchema,
    defaultValues: {
      usernameOrEmail: '',
      password: ''
    }
  });

  const { control, handleSubmit, formState: { errors, isSubmitting }, reset } = form;

  const onSubmit = handleSubmit(async (data) => {
    setError(null);

    try {
      const result = await login(data as unknown as LoginSchema);
      
      if (result.isSuccess) {
        onLoginSuccess();
      } else {
        setError(result.errorMessage || ERROR_MESSAGES.LOGIN_FAILED);
        reset({ usernameOrEmail: data.usernameOrEmail, password: '' });
      }
    } catch (error: unknown) {
      console.error('Login error:', error);
      
      let errorMessage: string = ERROR_MESSAGES.UNEXPECTED_ERROR;
      
      if (error instanceof Error) {
        errorMessage = error.message;
      } else if (typeof error === 'object' && error !== null && 'message' in error) {
        errorMessage = String(error.message);
      }
      
      setError(errorMessage);
      reset({ usernameOrEmail: data.usernameOrEmail, password: '' });
    }
  });

  return (
    <>
      <Header showAuth={false} showSearch={false} />
      <Paper elevation={3} sx={{ p: FORM_SETTINGS.PAPER_PADDING, maxWidth: FORM_SETTINGS.MAX_FORM_WIDTH, mx: 'auto', mt: FORM_SETTINGS.MARGIN_TOP }}>
      <Typography variant="h4" component="h1" gutterBottom align="center">
        {FORM_TITLES.LOGIN}
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
          name="usernameOrEmail"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.USERNAME_OR_EMAIL}
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.usernameOrEmail}
              helperText={errors.usernameOrEmail?.message}
              autoComplete={AUTOCOMPLETE_VALUES.USERNAME}
              aria-label={ARIA_LABELS.USERNAME_OR_EMAIL_INPUT}
              aria-required="true"
              aria-invalid={!!errors.usernameOrEmail}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
        />
        
        <Controller
          name="password"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.PASSWORD}
              type={showPassword ? 'text' : 'password'}
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.password}
              helperText={errors.password?.message}
              autoComplete={AUTOCOMPLETE_VALUES.CURRENT_PASSWORD}
              aria-label={ARIA_LABELS.PASSWORD_INPUT}
              aria-required="true"
              aria-invalid={!!errors.password}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={() => setShowPassword(!showPassword)}
                        onMouseDown={(e) => e.preventDefault()}
                        edge="end"
                        disabled={isSubmitting}
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  )
                }
              }}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
        />

        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1 }}>
          <Link
            component="button"
            type="button"
            variant="body2"
            onClick={onForgotPassword}
            disabled={isSubmitting}
            sx={{ 
              textDecoration: 'none',
              '&:hover': { textDecoration: 'underline' },
              cursor: isSubmitting ? 'not-allowed' : 'pointer',
              opacity: isSubmitting ? 0.5 : 1
            }}
          >
            {FORM_LABELS.FORGOT_PASSWORD}
          </Link>
        </Box>
        
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 2, mb: FORM_SETTINGS.MARGIN_BOTTOM }}
          disabled={isSubmitting}
          aria-label={isSubmitting ? ARIA_LABELS.LOGIN_BUTTON_LOADING : ARIA_LABELS.LOGIN_BUTTON}
        >
          {isSubmitting ? (
            <>
              <CircularProgress size={FORM_SETTINGS.LOADING_SPINNER_SIZE} sx={{ mr: 1 }} color="inherit" />
              {FORM_LABELS.LOGGING_IN}
            </>
          ) : (
            FORM_LABELS.LOGIN
          )}
        </Button>
      </Box>

      {/* Only show test credentials in development */}
      {process.env.NODE_ENV === 'development' && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <Typography variant="body2">
            <strong>{TEST_CREDENTIALS.LABEL}</strong><br />
            Username: "{TEST_CREDENTIALS.USERNAME}"<br />
            Password: "{TEST_CREDENTIALS.PASSWORD}"
          </Typography>
        </Alert>
      )}

      <Box sx={{ mt: 2, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          Don't have an account?{' '}
          <Link
            component="button"
            type="button"
            variant="body2"
            onClick={onRegister}
            disabled={isSubmitting}
            sx={{ cursor: 'pointer' }}
          >
            Register here
          </Link>
        </Typography>
      </Box>
    </Paper>
    </>
  );
};

export default LoginForm;
