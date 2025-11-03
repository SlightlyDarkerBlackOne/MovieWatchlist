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
import { useAuth } from '../../contexts/AuthContext';
import { useForms } from '../../hooks/useForms';
import { registerSchema, RegisterSchema } from '../../validation/schemas';
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

const RegisterForm: React.FC<RegisterFormProps> = ({ onRegisterSuccess, onBackToLogin }) => {
  const { register: registerUser } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const form = useForms<RegisterSchema>({
    schema: registerSchema,
    defaultValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: ''
    }
  });

  const { control, handleSubmit, formState: { errors, isSubmitting } } = form;

  const onSubmit = handleSubmit(async (data) => {
    setError(null);

    try {
      const registerData = {
        username: data.username,
        email: data.email,
        password: data.password
      };

      const result = await registerUser(registerData);
      
      if (result.isSuccess) {
        onRegisterSuccess();
      } else {
        setError(result.errorMessage || 'Registration failed. Please try again.');
      }
    } catch (error) {
      setError(ERROR_MESSAGES.UNEXPECTED_ERROR);
    }
  });

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

      <Box component="form" onSubmit={onSubmit} noValidate>
        <Controller
          name="username"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              fullWidth
              label={FORM_LABELS.USERNAME}
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.username}
              helperText={errors.username?.message}
              autoComplete={AUTOCOMPLETE_VALUES.USERNAME}
              inputProps={{
                'aria-label': ARIA_LABELS.USERNAME_INPUT,
                minLength: 3,
                maxLength: 50
              }}
              onChange={(e) => {
                field.onChange(e);
                if (error) setError(null);
              }}
            />
          )}
        />
        
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
              disabled={isSubmitting}
              error={!!errors.email}
              helperText={errors.email?.message}
              autoComplete={AUTOCOMPLETE_VALUES.EMAIL}
              inputProps={{
                'aria-label': ARIA_LABELS.EMAIL_INPUT
              }}
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
              helperText={errors.password?.message || 'Must be at least 8 characters with uppercase, lowercase, and number'}
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
                        disabled={isSubmitting}
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
              type={showConfirmPassword ? 'text' : 'password'}
              margin="normal"
              required
              disabled={isSubmitting}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
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
                        disabled={isSubmitting}
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
          sx={{ mt: 3, mb: 2 }}
          disabled={isSubmitting}
          aria-label={isSubmitting ? ARIA_LABELS.REGISTER_BUTTON_LOADING : ARIA_LABELS.REGISTER_BUTTON}
        >
          {isSubmitting ? (
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
            disabled={isSubmitting}
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

