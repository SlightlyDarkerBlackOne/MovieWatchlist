/**
 * Shared constants for authentication forms
 * Centralizes all text, labels, and common form values
 */

export const FORM_LABELS = {
  // Field labels
  USERNAME: 'Username',
  USERNAME_OR_EMAIL: 'Username or Email',
  EMAIL: 'Email Address',
  PASSWORD: 'Password',
  NEW_PASSWORD: 'New Password',
  CONFIRM_PASSWORD: 'Confirm Password',
  
  // Button labels
  LOGIN: 'Login',
  REGISTER: 'Register',
  LOGOUT: 'Logout',
  SEND_RESET_LINK: 'Send Reset Link',
  RESET_PASSWORD: 'Reset Password',
  BACK_TO_LOGIN: 'Back to Login',
  FORGOT_PASSWORD: 'Forgot password?',
  
  // Loading states
  LOGGING_IN: 'Logging in...',
  REGISTERING: 'Registering...',
  SENDING: 'Sending...',
  RESETTING: 'Resetting...',
} as const;

export const FORM_TITLES = {
  LOGIN: 'Login',
  REGISTER: 'Register',
  FORGOT_PASSWORD: 'Forgot Password',
  RESET_PASSWORD: 'Reset Password',
} as const;

export const FORM_DESCRIPTIONS = {
  FORGOT_PASSWORD: "Enter your email address and we'll send you a link to reset your password.",
  RESET_PASSWORD: 'Enter your new password below.',
} as const;

export const ERROR_MESSAGES = {
  // Generic errors
  UNEXPECTED_ERROR: 'An unexpected error occurred. Please try again.',
  NETWORK_ERROR: 'Network error. Please check your connection.',
  
  // Login errors
  LOGIN_FAILED: 'Login failed. Please check your credentials.',
  
  // Password reset errors
  RESET_FAILED: 'Failed to reset password. The link may have expired.',
  SEND_RESET_FAILED: 'Failed to send reset email',
  
  // Validation errors
  CONFIRM_PASSWORD_REQUIRED: 'Please confirm your password',
  PASSWORDS_DO_NOT_MATCH: 'Passwords do not match',
} as const;

export const SUCCESS_MESSAGES = {
  PASSWORD_RESET_EMAIL_SENT: 'If the email exists, a password reset link has been sent. Please check your inbox.',
  PASSWORD_RESET_SUCCESS: 'Password reset successfully! You can now login with your new password.',
  LOGIN_SUCCESS: 'Login successful!',
} as const;

export const ARIA_LABELS = {
  // Form fields
  USERNAME_INPUT: 'Username',
  USERNAME_OR_EMAIL_INPUT: 'Username or Email',
  EMAIL_INPUT: 'Email Address',
  PASSWORD_INPUT: 'Password',
  NEW_PASSWORD_INPUT: 'New Password',
  CONFIRM_PASSWORD_INPUT: 'Confirm Password',
  
  // Buttons
  LOGIN_BUTTON: 'Login',
  LOGIN_BUTTON_LOADING: 'Logging in',
  REGISTER_BUTTON: 'Register',
  REGISTER_BUTTON_LOADING: 'Registering',
  SEND_RESET_LINK_BUTTON: 'Send reset link',
  SEND_RESET_LINK_BUTTON_LOADING: 'Sending reset link',
  RESET_PASSWORD_BUTTON: 'Reset password',
  RESET_PASSWORD_BUTTON_LOADING: 'Resetting password',
} as const;

export const AUTOCOMPLETE_VALUES = {
  USERNAME: 'username',
  EMAIL: 'email',
  CURRENT_PASSWORD: 'current-password',
  NEW_PASSWORD: 'new-password',
} as const;

export const FORM_SETTINGS = {
  // Form layout
  MAX_FORM_WIDTH: 400,
  PAPER_PADDING: 4,
  
  // Margins
  MARGIN_TOP: 4,
  MARGIN_BOTTOM: 2,
  
  // Spinner size
  LOADING_SPINNER_SIZE: 20,
} as const;

export const TEST_CREDENTIALS = {
  USERNAME: 'testuser',
  PASSWORD: 'TestPassword123!',
  LABEL: 'Test credentials:',
} as const;

