/**
 * Tests for authentication service
 */

import authService from './authService';
import api from './api';
import { mockLoginCredentials, mockRegisterData, mockUser } from '../__tests__/fixtures/authFixtures';
import { TestConstants } from '../__tests__/TestConstants';

// Mock dependencies
jest.mock('./api');

const mockedApi = api as jest.Mocked<typeof api>;

describe('AuthService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should successfully login and return user info', async () => {
      const mockResponse = { user: mockUser, expiresAt: new Date(Date.now() + 3600000).toISOString() };
      mockedApi.post.mockResolvedValue({ data: mockResponse });

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(true);
      expect(result.user).toEqual(mockUser);
      expect(result.expiresAt).toBeDefined();
    });

    it('should handle login failure', async () => {
      mockedApi.post.mockRejectedValue({
        response: { data: { error: 'Invalid credentials' } }
      });

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(false);
      expect(result.errorMessage).toBe('Invalid credentials');
    });

    it('should handle network errors', async () => {
      mockedApi.post.mockRejectedValue(new Error('Network error'));

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(false);
      expect(result.errorMessage).toContain('Login failed');
    });
  });

  describe('register', () => {
    it('should successfully register user and return user info', async () => {
      const mockResponse = { user: mockUser, expiresAt: new Date(Date.now() + 3600000).toISOString() };
      mockedApi.post.mockResolvedValue({ data: mockResponse });

      const result = await authService.register(mockRegisterData);

      expect(result.isSuccess).toBe(true);
      expect(result.user).toEqual(mockUser);
    });

    it('should handle validation errors', async () => {
      mockedApi.post.mockRejectedValue({
        response: {
          data: {
            errors: {
              username: ['Username already exists'],
              email: ['Email already in use'],
            }
          }
        }
      });

      const result = await authService.register(mockRegisterData);

      expect(result.isSuccess).toBe(false);
      expect(result.errorMessage).toContain('Username already exists');
    });
  });

  describe('logout', () => {
    it('should call logout endpoint and return true on success', async () => {
      mockedApi.post.mockResolvedValue({ data: { message: 'Logged out' } });

      const result = await authService.logout();

      expect(result).toBe(true);
      expect(mockedApi.post).toHaveBeenCalledWith('/Auth/logout');
    });

    it('should return false on API failure but still complete', async () => {
      mockedApi.post.mockRejectedValue(new Error('API error'));

      const result = await authService.logout();

      expect(result).toBe(false);
    });
  });

  describe('validateToken', () => {
    it('should return false (not supported)', async () => {
      const result = await authService.validateToken();
      expect(result).toBe(false);
    });
  });

  describe('refreshToken', () => {
    it('should throw error (not supported)', async () => {
      await expect(authService.refreshToken('token')).rejects.toThrow(TestConstants.UI.NotSupported);
    });
  });

  describe('isAuthenticated', () => {
    it('should return false (no token access)', () => {
      expect(authService.isAuthenticated()).toBe(false);
    });
  });

  describe('getCurrentUser', () => {
    it('should return null (no localStorage access)', () => {
      const user = authService.getCurrentUser();
      expect(user).toBeNull();
    });
  });

  describe('getToken', () => {
    it('should return null (no token access)', () => {
      const token = authService.getToken();
      expect(token).toBeNull();
    });
  });
});


