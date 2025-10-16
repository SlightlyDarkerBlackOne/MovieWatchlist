/**
 * Test fixtures for authentication data
 */

import { User, LoginCredentials, RegisterData, AuthenticationResult } from '../../types/auth.types';

export const mockUser: User = {
  id: 1,
  username: 'testuser',
  email: 'test@example.com',
};

export const mockLoginCredentials: LoginCredentials = {
  username: 'testuser',
  password: 'Test123!@#',
};

export const mockRegisterData: RegisterData = {
  username: 'newuser',
  email: 'newuser@example.com',
  password: 'NewUser123!@#',
  confirmPassword: 'NewUser123!@#',
};

export const mockAuthenticationResult: AuthenticationResult = {
  isSuccess: true,
  token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6InRlc3R1c2VyIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwiZXhwIjoxNzM5OTk5OTk5fQ.test',
  refreshToken: 'mock-refresh-token-12345',
  user: mockUser,
};

export const mockFailedAuthenticationResult: AuthenticationResult = {
  isSuccess: false,
  errorMessage: 'Invalid username or password',
};

export const mockJwtToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6InRlc3R1c2VyIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwiZXhwIjoxNzM5OTk5OTk5fQ.test';

export const mockExpiredJwtToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6InRlc3R1c2VyIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwiZXhwIjoxNjAwMDAwMDAwfQ.test';


