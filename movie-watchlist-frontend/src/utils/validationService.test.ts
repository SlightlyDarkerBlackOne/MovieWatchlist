/**
 * Tests for validation service
 */

import { validationService } from './validationService';

describe('ValidationService', () => {
  describe('validateEmail', () => {
    it('should validate correct email addresses', () => {
      expect(validationService.validateEmail('test@example.com')).toBe(true);
      expect(validationService.validateEmail('user.name@domain.co.uk')).toBe(true);
      expect(validationService.validateEmail('first+last@test.com')).toBe(true);
    });

    it('should reject invalid email addresses', () => {
      expect(validationService.validateEmail('invalid')).toBe(false);
      expect(validationService.validateEmail('no-at-sign.com')).toBe(false);
      expect(validationService.validateEmail('@no-local-part.com')).toBe(false);
      expect(validationService.validateEmail('no-domain@.com')).toBe(false);
      expect(validationService.validateEmail('')).toBe(false);
    });

    it('should handle edge cases', () => {
      expect(validationService.validateEmail(' test@example.com ')).toBe(false); // spaces
      expect(validationService.validateEmail('test@example')).toBe(false); // no TLD
    });
  });

  describe('validatePassword', () => {
    it('should validate strong passwords', () => {
      const result = validationService.validatePassword('Test123!@#');
      expect(result.isValid).toBe(true);
      expect(result.errors).toHaveLength(0);
    });

    it('should reject password that is too short', () => {
      const result = validationService.validatePassword('Abc1!');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Password must be at least 8 characters long');
    });

    it('should reject password without uppercase letter', () => {
      const result = validationService.validatePassword('test123!@#');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Password must contain at least one uppercase letter');
    });

    it('should reject password without lowercase letter', () => {
      const result = validationService.validatePassword('TEST123!@#');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Password must contain at least one lowercase letter');
    });

    it('should reject password without number', () => {
      const result = validationService.validatePassword('TestTest!@#');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Password must contain at least one number');
    });

    it('should reject password without special character', () => {
      const result = validationService.validatePassword('Test1234');
      expect(result.isValid).toBe(false);
      expect(result.errors).toContain('Password must contain at least one special character');
    });

    it('should return multiple errors for weak password', () => {
      const result = validationService.validatePassword('abc');
      expect(result.isValid).toBe(false);
      expect(result.errors.length).toBeGreaterThan(1);
    });
  });

  describe('sanitizeInput', () => {
    it('should trim whitespace', () => {
      expect(validationService.sanitizeInput('  test  ')).toBe('test');
    });

    it('should remove script tags', () => {
      expect(validationService.sanitizeInput('<script>alert("xss")</script>test')).toBe('test');
    });

    it('should remove HTML tags', () => {
      expect(validationService.sanitizeInput('<div>test</div>')).toBe('test');
      expect(validationService.sanitizeInput('<p>Hello <b>World</b></p>')).toBe('Hello World');
    });

    it('should handle empty string', () => {
      expect(validationService.sanitizeInput('')).toBe('');
    });

    it('should handle string with only whitespace', () => {
      expect(validationService.sanitizeInput('   ')).toBe('');
    });

    it('should preserve safe content', () => {
      expect(validationService.sanitizeInput('This is a safe string!')).toBe('This is a safe string!');
    });
  });
});


