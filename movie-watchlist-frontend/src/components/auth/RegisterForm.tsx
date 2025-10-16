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
import { RegisterData } from '../../types/auth.types';
import ValidationService from '../../utils/validationService';
import { useAuth } from '../../contexts/AuthContext';
import { 
  FORM_LABELS, 
  FORM_TITLES, 
  ERROR_MESSAGES, 
  ARIA_LABELS, 
  AUTOCOMPLETE_VALUES,
  FORM_SETTINGS
} from '../../constants/formConstants';

interface RegisterFormProps {
  onRegisterSuccess: () => void;
  onBackToLogin: () => void;
}

interface FormErrors {
  username?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
}

const RegisterForm: React.FC<RegisterFormProps> = ({ onRegisterSuccess, onBackToLogin }) => {
  const { register } = useAuth();
  const [formData, setFormData] = useState<RegisterData & { confirmPassword: string }>({
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear field-specific error when user starts typing
    if (fieldErrors[name as keyof FormErrors]) {
      setFieldErrors(prev => ({
        ...prev,
        [name]: undefined
      }));
    }
    
    // Clear general error when user makes changes
    if (error) {
      setError(null);
    }
  };

  const validateForm = (): boolean => {
    const errors: FormErrors = {};
    let isValid = true;

    // Validate username
    const usernameValidation = ValidationService.validateUsernameField(formData.username);
    if (!usernameValidation.isValid) {
      errors.username = usernameValidation.error || 'Invalid username';
      isValid = false;
    }

    // Validate email
    const emailValidation = ValidationService.validateEmailField(formData.email);
    if (!emailValidation.isValid) {
      errors.email = emailValidation.error || 'Invalid email address';
      isValid = false;
    }

    // Validate password
    const passwordValidation = ValidationService.validateLoginPassword(formData.password);
    if (!passwordValidation.isValid) {
      errors.password = passwordValidation.error || 'Invalid password';
      isValid = false;
    }

    // Validate confirm password
    if (!formData.confirmPassword) {
      errors.confirmPassword = ERROR_MESSAGES.CONFIRM_PASSWORD_REQUIRED;
      isValid = false;
    } else if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = ERROR_MESSAGES.PASSWORDS_DO_NOT_MATCH;
      isValid = false;
    }

    setFieldErrors(errors);
    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const registerData: RegisterData = {
        username: formData.username,
        email: formData.email,
        password: formData.password
      };

      const result = await register(registerData);
      
      if (result.isSuccess) {
        onRegisterSuccess();
      } else {
        setError(result.errorMessage || 'Registration failed. Please try again.');
      }
    } catch (error) {
      setError(ERROR_MESSAGES.UNEXPECTED_ERROR);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper 
      elevation={3} 
      sx={{ 
        p: FORM_SETTINGS.PAPER_PADDING, 
        maxWidth: FORM_SETTINGS.MAX_FORM_WIDTH, 
        mx: 'auto', 
        mt: FORM_SETTINGS.MARGIN_TOP 
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom align="center">
        {FORM_TITLES.REGISTER}
      </Typography>
      
      <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 3 }}>
        Create your account to start managing your movie watchlist
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit} noValidate>
        <TextField
          fullWidth
          label={FORM_LABELS.USERNAME}
          name="username"
          value={formData.username}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.username}
          helperText={fieldErrors.username}
          autoComplete={AUTOCOMPLETE_VALUES.USERNAME}
          inputProps={{
            'aria-label': ARIA_LABELS.USERNAME_INPUT,
            minLength: 3,
            maxLength: 50
          }}
        />
        
        <TextField
          fullWidth
          label={FORM_LABELS.EMAIL}
          name="email"
          type="email"
          value={formData.email}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.email}
          helperText={fieldErrors.email}
          autoComplete={AUTOCOMPLETE_VALUES.EMAIL}
          inputProps={{
            'aria-label': ARIA_LABELS.EMAIL_INPUT
          }}
        />
        
        <TextField
          fullWidth
          label={FORM_LABELS.PASSWORD}
          name="password"
          type={showPassword ? 'text' : 'password'}
          value={formData.password}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.password}
          helperText={fieldErrors.password || 'Must be at least 8 characters with uppercase, lowercase, and number'}
          autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
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
          inputProps={{
            'aria-label': ARIA_LABELS.PASSWORD_INPUT,
            minLength: 8
          }}
        />

        <TextField
          fullWidth
          label={FORM_LABELS.CONFIRM_PASSWORD}
          name="confirmPassword"
          type={showConfirmPassword ? 'text' : 'password'}
          value={formData.confirmPassword}
          onChange={handleInputChange}
          margin="normal"
          required
          disabled={loading}
          error={!!fieldErrors.confirmPassword}
          helperText={fieldErrors.confirmPassword}
          autoComplete={AUTOCOMPLETE_VALUES.NEW_PASSWORD}
          slotProps={{
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="toggle confirm password visibility"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    onMouseDown={(e) => e.preventDefault()}
                    edge="end"
                    disabled={loading}
                  >
                    {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              )
            }
          }}
          inputProps={{
            'aria-label': ARIA_LABELS.CONFIRM_PASSWORD_INPUT,
            minLength: 8
          }}
        />
        
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
          disabled={loading}
          aria-label={loading ? ARIA_LABELS.REGISTER_BUTTON_LOADING : ARIA_LABELS.REGISTER_BUTTON}
        >
          {loading ? (
            <>
              <CircularProgress 
                size={FORM_SETTINGS.LOADING_SPINNER_SIZE} 
                sx={{ mr: 1 }} 
                color="inherit"
              />
              {FORM_LABELS.REGISTERING}
            </>
          ) : (
            FORM_LABELS.REGISTER
          )}
        </Button>
      </Box>

      <Box sx={{ mt: 2, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          Already have an account?{' '}
          <Link
            component="button"
            variant="body2"
            onClick={onBackToLogin}
            disabled={loading}
            sx={{ cursor: 'pointer' }}
          >
            {FORM_LABELS.BACK_TO_LOGIN}
          </Link>
        </Typography>
      </Box>
    </Paper>
  );
};

export default RegisterForm;

