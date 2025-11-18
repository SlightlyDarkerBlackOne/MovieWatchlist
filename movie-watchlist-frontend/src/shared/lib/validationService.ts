/**
 * Frontend validation service that mirrors backend validation rules
 * Based on MovieWatchlist.Core.Validation.InputValidationService
 */

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface FieldValidationResult {
  isValid: boolean;
  error?: string;
}

export class ValidationService {
  // Validation regex patterns - matching backend exactly
  private static readonly EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$/;
  private static readonly USERNAME_REGEX = /^[a-zA-Z0-9_-]{3,50}$/;
  
  // Password regex: At least 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
  private static readonly PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

  // Public constants for use in components
  public static readonly MAX_EMAIL_LENGTH = 100;
  public static readonly MAX_PASSWORD_LENGTH = 100;
  public static readonly MIN_USERNAME_LENGTH = 3;
  public static readonly MAX_USERNAME_LENGTH = 50;
  public static readonly MIN_PASSWORD_LENGTH = 8;
  public static readonly PASSWORD_SPECIAL_CHARS = '@$!%*?&';

  /**
   * Validates email format and length
   */
  static isValidEmail(email: string | null | undefined): boolean {
    if (!email || !email.trim()) return false;
    return this.EMAIL_REGEX.test(email) && email.length <= this.MAX_EMAIL_LENGTH;
  }

  /**
   * Validates username format and length
   * Must be 3-50 characters, alphanumeric with underscores and hyphens only
   */
  static isValidUsername(username: string | null | undefined): boolean {
    if (!username || !username.trim()) return false;
    return this.USERNAME_REGEX.test(username);
  }

  /**
   * Validates password strength
   * Must be at least 8 characters with uppercase, lowercase, number, and special character
   */
  static isValidPassword(password: string | null | undefined): boolean {
    if (!password || !password.trim()) return false;
    return this.PASSWORD_REGEX.test(password) && password.length <= this.MAX_PASSWORD_LENGTH;
  }

  /**
   * Validates username or email field (for login)
   */
  static validateUsernameOrEmail(value: string): FieldValidationResult {
    if (!value || !value.trim()) {
      return {
        isValid: false,
        error: 'Username or email is required'
      };
    }

    const trimmedValue = value.trim();

    // Check if it looks like an email
    if (trimmedValue.includes('@')) {
      if (!this.isValidEmail(trimmedValue)) {
        return {
          isValid: false,
          error: `Invalid email format or exceeds ${this.MAX_EMAIL_LENGTH} characters`
        };
      }
    } else {
      // It's a username
      if (!this.isValidUsername(trimmedValue)) {
        return {
          isValid: false,
          error: `Username must be ${this.MIN_USERNAME_LENGTH}-${this.MAX_USERNAME_LENGTH} characters (letters, numbers, _, -)`
        };
      }
    }

    return { isValid: true };
  }

  /**
   * Validates password field (for login)
   */
  static validateLoginPassword(password: string): FieldValidationResult {
    if (!password) {
      return {
        isValid: false,
        error: 'Password is required'
      };
    }

    // For login, we don't enforce strict password requirements
    // (user might have registered with old requirements)
    // Just check minimum length
    if (password.length < this.MIN_PASSWORD_LENGTH) {
      return {
        isValid: false,
        error: `Password must be at least ${this.MIN_PASSWORD_LENGTH} characters`
      };
    }

    return { isValid: true };
  }

  /**
   * Validates password field for registration/reset
   * Enforces all password strength requirements
   */
  static validatePasswordStrength(password: string): FieldValidationResult {
    if (!password) {
      return {
        isValid: false,
        error: 'Password is required'
      };
    }

    if (!this.isValidPassword(password)) {
      const errors: string[] = [];
      
      if (password.length < this.MIN_PASSWORD_LENGTH) {
        errors.push(`at least ${this.MIN_PASSWORD_LENGTH} characters`);
      }
      if (!/[a-z]/.test(password)) {
        errors.push('one lowercase letter');
      }
      if (!/[A-Z]/.test(password)) {
        errors.push('one uppercase letter');
      }
      if (!/\d/.test(password)) {
        errors.push('one number');
      }
      if (!/[@$!%*?&]/.test(password)) {
        errors.push('one special character (@$!%*?&)');
      }
      if (password.length > this.MAX_PASSWORD_LENGTH) {
        errors.push(`maximum ${this.MAX_PASSWORD_LENGTH} characters`);
      }

      return {
        isValid: false,
        error: `Password must contain ${errors.join(', ')}`
      };
    }

    return { isValid: true };
  }

  /**
   * Validates email field for registration
   */
  static validateEmailField(email: string): FieldValidationResult {
    if (!email || !email.trim()) {
      return {
        isValid: false,
        error: 'Email is required'
      };
    }

    if (!this.isValidEmail(email)) {
      return {
        isValid: false,
        error: `Invalid email format or exceeds ${this.MAX_EMAIL_LENGTH} characters`
      };
    }

    return { isValid: true };
  }

  /**
   * Validates username field for registration
   */
  static validateUsernameField(username: string): FieldValidationResult {
    if (!username || !username.trim()) {
      return {
        isValid: false,
        error: 'Username is required'
      };
    }

    if (!this.isValidUsername(username)) {
      return {
        isValid: false,
        error: `Username must be ${this.MIN_USERNAME_LENGTH}-${this.MAX_USERNAME_LENGTH} characters (letters, numbers, _, -)`
      };
    }

    return { isValid: true };
  }

  /**
   * Sanitizes user input by trimming whitespace and removing HTML tags
   */
  static sanitizeInput(input: string | null | undefined): string {
    if (!input) return '';
    // Remove script tags and their content first
    let sanitized = input.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '');
    // Remove all other HTML tags
    sanitized = sanitized.replace(/<[^>]*>/g, '');
    // Trim whitespace
    return sanitized.trim();
  }

  /**
   * Returns a formatted string describing password requirements
   */
  static getPasswordRequirementsText(): string {
    return `at least ${this.MIN_PASSWORD_LENGTH} characters with uppercase, lowercase, number, and special character (${this.PASSWORD_SPECIAL_CHARS})`;
  }

  /**
   * Returns a formatted string describing username requirements
   */
  static getUsernameRequirementsText(): string {
    return `${this.MIN_USERNAME_LENGTH}-${this.MAX_USERNAME_LENGTH} characters (letters, numbers, _, -)`;
  }
}

// Export default class
export default ValidationService;

// Helper function to convert FieldValidationResult to ValidationResult for password
function validatePasswordWithErrors(password: string): ValidationResult {
  if (!password) {
    return {
      isValid: false,
      errors: ['Password is required']
    };
  }

  const errors: string[] = [];
  
  if (password.length < ValidationService.MIN_PASSWORD_LENGTH) {
    errors.push(`Password must be at least ${ValidationService.MIN_PASSWORD_LENGTH} characters long`);
  }
  if (!/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase letter');
  }
  if (!/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase letter');
  }
  if (!/\d/.test(password)) {
    errors.push('Password must contain at least one number');
  }
  if (!/[@$!%*?&]/.test(password)) {
    errors.push('Password must contain at least one special character');
  }
  if (password.length > ValidationService.MAX_PASSWORD_LENGTH) {
    errors.push(`Password must not exceed ${ValidationService.MAX_PASSWORD_LENGTH} characters`);
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

// Export instance for convenience (used in tests and components)
export const validationService = {
  validateEmail: ValidationService.isValidEmail.bind(ValidationService),
  validatePassword: validatePasswordWithErrors,
  sanitizeInput: ValidationService.sanitizeInput.bind(ValidationService),
};

