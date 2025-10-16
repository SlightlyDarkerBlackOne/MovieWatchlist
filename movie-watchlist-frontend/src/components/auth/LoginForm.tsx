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
import Header from '../common/Header';
import { LoginCredentials } from '../../types/auth.types';
import ValidationService from '../../utils/validationService';
import { useAuth } from '../../contexts/AuthContext';
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

interface FormErrors {
  usernameOrEmail?: string;
  password?: string;
}

const LoginForm: React.FC<LoginFormProps> = ({ onLoginSuccess, onForgotPassword, onRegister }) => {
  const { login } = useAuth();
  const [credentials, setCredentials] = useState<LoginCredentials>({
    usernameOrEmail: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});
  const [showPassword, setShowPassword] = useState(false);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setCredentials(prev => ({
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
    
    // Validate username/email using backend validation rules
    const usernameOrEmailValidation = ValidationService.validateUsernameOrEmail(
      credentials.usernameOrEmail
    );
    if (!usernameOrEmailValidation.isValid) {
      newErrors.usernameOrEmail = usernameOrEmailValidation.error;
    }
    
    // Validate password
    const passwordValidation = ValidationService.validateLoginPassword(
      credentials.password
    );
    if (!passwordValidation.isValid) {
      newErrors.password = passwordValidation.error;
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
      const result = await login(credentials);
      
      if (result.isSuccess) {
        onLoginSuccess();
      } else {
        setError(result.errorMessage || ERROR_MESSAGES.LOGIN_FAILED);
        // Clear password on failed login for security
        setCredentials(prev => ({ ...prev, password: '' }));
      }
    } catch (error: unknown) {
      console.error('Login error:', error);
      
      // Better error message based on error type
      let errorMessage: string = ERROR_MESSAGES.UNEXPECTED_ERROR;
      
      if (error instanceof Error) {
        errorMessage = error.message;
      } else if (typeof error === 'object' && error !== null && 'message' in error) {
        errorMessage = String(error.message);
      }
      
      setError(errorMessage);
      // Clear password on error
      setCredentials(prev => ({ ...prev, password: '' }));
    } finally {
      setLoading(false);
    }
  };

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

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <TextField
          fullWidth
          label={FORM_LABELS.USERNAME_OR_EMAIL}
          name="usernameOrEmail"
          value={credentials.usernameOrEmail}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.usernameOrEmail}
          helperText={fieldErrors.usernameOrEmail}
          autoComplete={AUTOCOMPLETE_VALUES.USERNAME}
          aria-label={ARIA_LABELS.USERNAME_OR_EMAIL_INPUT}
          aria-required="true"
          aria-invalid={!!fieldErrors.usernameOrEmail}
        />
        
        <TextField
          fullWidth
          label={FORM_LABELS.PASSWORD}
          name="password"
          type={showPassword ? 'text' : 'password'}
          value={credentials.password}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.password}
          helperText={fieldErrors.password}
          autoComplete={AUTOCOMPLETE_VALUES.CURRENT_PASSWORD}
          aria-label={ARIA_LABELS.PASSWORD_INPUT}
          aria-required="true"
          aria-invalid={!!fieldErrors.password}
          slotProps={{
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle password visibility"
                    onClick={() => setShowPassword(!showPassword)}
                    onMouseDown={(e) => e.preventDefault()}
                    edge="end"
                    disabled={loading}
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              )
            }
          }}
        />

        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1 }}>
          <Link
            component="button"
            type="button"
            variant="body2"
            onClick={onForgotPassword}
            disabled={loading}
            sx={{ 
              textDecoration: 'none',
              '&:hover': { textDecoration: 'underline' },
              cursor: loading ? 'not-allowed' : 'pointer',
              opacity: loading ? 0.5 : 1
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
          disabled={loading}
          aria-label={loading ? ARIA_LABELS.LOGIN_BUTTON_LOADING : ARIA_LABELS.LOGIN_BUTTON}
        >
          {loading ? (
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
            disabled={loading}
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
