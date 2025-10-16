/**
 * Tests for authentication service
 */

import authService from './authService';
import api from './api';
import { mockLoginCredentials, mockRegisterData, mockAuthenticationResult, mockUser } from '../__tests__/fixtures/authFixtures';
import { STORAGE_KEYS } from '../utils/constants';

// Mock dependencies
jest.mock('./api');

const mockedApi = api as jest.Mocked<typeof api>;

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value;
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

describe('AuthService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  describe('login', () => {
    it('should successfully login and store tokens', async () => {
      mockedApi.post.mockResolvedValue({ data: mockAuthenticationResult });

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(true);
      expect(result.token).toBeDefined();
      expect(result.user).toEqual(mockUser);
      
      // Verify tokens stored in localStorage
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBe(mockAuthenticationResult.token);
      expect(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)).toBe(mockAuthenticationResult.refreshToken);
    });

    it('should handle login failure', async () => {
      mockedApi.post.mockRejectedValue({
        response: { data: { message: 'Invalid credentials' } }
      });

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(false);
      expect(result.errorMessage).toBe('Invalid credentials');
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull();
    });

    it('should handle network errors', async () => {
      mockedApi.post.mockRejectedValue(new Error('Network error'));

      const result = await authService.login(mockLoginCredentials);

      expect(result.isSuccess).toBe(false);
      expect(result.errorMessage).toContain('Login failed');
    });
  });

  describe('register', () => {
    it('should successfully register user and store tokens', async () => {
      mockedApi.post.mockResolvedValue({ data: mockAuthenticationResult });

      const result = await authService.register(mockRegisterData);

      expect(result.isSuccess).toBe(true);
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeDefined();
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
    it('should clear tokens and return true on success', async () => {
      localStorage.setItem(STORAGE_KEYS.TOKEN, 'test-token');
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, 'test-refresh');
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(mockUser));
      
      mockedApi.post.mockResolvedValue({ data: { success: true } });

      const result = await authService.logout();

      expect(result).toBe(true);
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull();
      expect(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)).toBeNull();
      expect(localStorage.getItem(STORAGE_KEYS.USER)).toBeNull();
    });

    it('should clear tokens even on API failure', async () => {
      localStorage.setItem(STORAGE_KEYS.TOKEN, 'test-token');
      mockedApi.post.mockRejectedValue(new Error('API error'));

      const result = await authService.logout();

      // Should still clear tokens locally
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull();
      expect(result).toBe(false);
    });
  });

  describe('validateToken', () => {
    it('should return false when no token exists', async () => {
      const result = await authService.validateToken();
      expect(result).toBe(false);
    });

    it('should return true for valid token', async () => {
      // Create a valid JWT token (expires in future)
      const futureTimestamp = Math.floor(Date.now() / 1000) + 3600; // 1 hour from now
      const payload = btoa(JSON.stringify({ exp: futureTimestamp }));
      const validToken = `header.${payload}.signature`;
      
      localStorage.setItem(STORAGE_KEYS.TOKEN, validToken);

      const result = await authService.validateToken();
      expect(result).toBe(true);
    });

    it('should return false for expired token', async () => {
      // Create an expired JWT token
      const pastTimestamp = Math.floor(Date.now() / 1000) - 3600; // 1 hour ago
      const payload = btoa(JSON.stringify({ exp: pastTimestamp }));
      const expiredToken = `header.${payload}.signature`;
      
      localStorage.setItem(STORAGE_KEYS.TOKEN, expiredToken);

      const result = await authService.validateToken();
      expect(result).toBe(false);
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull(); // Should clear
    });

    it('should handle malformed token', async () => {
      localStorage.setItem(STORAGE_KEYS.TOKEN, 'invalid-token');

      const result = await authService.validateToken();
      expect(result).toBe(false);
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull();
    });
  });

  describe('refreshToken', () => {
    it('should refresh token successfully', async () => {
      const newAuthResult = {
        ...mockAuthenticationResult,
        token: 'new-token',
        refreshToken: 'new-refresh-token',
      };
      
      mockedApi.post.mockResolvedValue({ data: newAuthResult });

      const result = await authService.refreshToken('old-refresh-token');

      expect(result.token).toBe('new-token');
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBe('new-token');
      expect(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)).toBe('new-refresh-token');
    });

    it('should clear tokens on refresh failure', async () => {
      localStorage.setItem(STORAGE_KEYS.TOKEN, 'old-token');
      mockedApi.post.mockRejectedValue({
        response: { data: { message: 'Invalid refresh token' } }
      });

      await expect(authService.refreshToken('invalid')).rejects.toThrow();
      expect(localStorage.getItem(STORAGE_KEYS.TOKEN)).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem(STORAGE_KEYS.TOKEN, 'test-token');
      expect(authService.isAuthenticated()).toBe(true);
    });

    it('should return false when no token exists', () => {
      expect(authService.isAuthenticated()).toBe(false);
    });
  });

  describe('getCurrentUser', () => {
    it('should return user from localStorage', () => {
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(mockUser));
      
      const user = authService.getCurrentUser();
      expect(user).toEqual(mockUser);
    });

    it('should return null when no user stored', () => {
      const user = authService.getCurrentUser();
      expect(user).toBeNull();
    });

    it('should handle invalid JSON in localStorage', () => {
      localStorage.setItem(STORAGE_KEYS.USER, 'invalid-json');
      
      const user = authService.getCurrentUser();
      expect(user).toBeNull();
    });
  });
});


