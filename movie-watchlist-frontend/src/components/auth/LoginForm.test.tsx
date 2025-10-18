/**
 * Tests for LoginForm component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter } from 'react-router-dom';
import LoginForm from './LoginForm';
import { appTheme } from '../../theme';
import { useAuth } from '../../contexts/AuthContext';

// Mock the useAuth hook
jest.mock('../../contexts/AuthContext', () => ({
  useAuth: jest.fn(),
}));

// Mock Header component
jest.mock('../common/Header', () => {
  return function MockHeader() {
    return <div data-testid="mock-header">Header</div>;
  };
});

const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('LoginForm', () => {
  const mockLoginFn = jest.fn();
  const mockOnLoginSuccess = jest.fn();
  const mockOnForgotPassword = jest.fn();
  const mockOnRegister = jest.fn();

  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <ThemeProvider theme={appTheme}>
        <BrowserRouter>
          {ui}
        </BrowserRouter>
      </ThemeProvider>
    );
  };

  // Helper to get password input by name attribute (avoid ambiguity with multiple password fields)
  const getPasswordInput = () => document.querySelector<HTMLInputElement>('input[name="password"]')!;

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseAuth.mockReturnValue({
      login: mockLoginFn,
      register: jest.fn(),
      logout: jest.fn(),
      forgotPassword: jest.fn(),
      resetPassword: jest.fn(),
      validateToken: jest.fn(),
      isAuthenticated: jest.fn(() => false),
      getToken: jest.fn(() => null),
      user: null,
      isLoading: false,
    });
  });

  it('should render login form with all fields', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /username or email/i })).toBeInTheDocument();
    expect(getPasswordInput()).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^login$/i })).toBeInTheDocument();
  });

  it('should show forgot password link', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const forgotPasswordLink = screen.getByText(/forgot password/i);
    expect(forgotPasswordLink).toBeInTheDocument();
  });

  it('should show register link', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    expect(screen.getByText(/don't have an account/i)).toBeInTheDocument();
    expect(screen.getByText(/register here/i)).toBeInTheDocument();
  });

  it('should update input fields when typing', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i }) as HTMLInputElement;
    const passwordInput = getPasswordInput() as HTMLInputElement;

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'TestPassword123' } });

    expect(usernameInput.value).toBe('testuser');
    expect(passwordInput.value).toBe('TestPassword123');
  });

  it('should toggle password visibility', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const passwordInput = getPasswordInput() as HTMLInputElement;
    const toggleButton = screen.getByLabelText(/toggle password visibility/i);

    expect(passwordInput.type).toBe('password');

    fireEvent.click(toggleButton);
    expect(passwordInput.type).toBe('text');

    fireEvent.click(toggleButton);
    expect(passwordInput.type).toBe('password');
  });

  it('should show validation errors for empty fields', async () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/username or email is required/i)).toBeInTheDocument();
      expect(screen.getByText(/password is required/i)).toBeInTheDocument();
    });

    expect(mockLoginFn).not.toHaveBeenCalled();
  });

  it('should show validation error for short username', async () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'ab' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/username must be 3-50 characters/i)).toBeInTheDocument();
    });

    expect(mockLoginFn).not.toHaveBeenCalled();
  });

  it('should submit form with valid credentials and call onLoginSuccess', async () => {
    mockLoginFn.mockResolvedValue({
      isSuccess: true,
      user: { id: 1, username: 'testuser', email: 'test@example.com' },
    });

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockLoginFn).toHaveBeenCalledWith({
        usernameOrEmail: 'testuser',
        password: 'Password123',
      });
      expect(mockOnLoginSuccess).toHaveBeenCalled();
    });
  });

  it('should show error message on login failure', async () => {
    const errorMessage = 'Invalid credentials';
    mockLoginFn.mockResolvedValue({
      isSuccess: false,
      errorMessage,
    });

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'WrongPassword123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });

    expect(mockOnLoginSuccess).not.toHaveBeenCalled();
  });

  it('should clear password field on login failure', async () => {
    mockLoginFn.mockResolvedValue({
      isSuccess: false,
      errorMessage: 'Invalid credentials',
    });

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i }) as HTMLInputElement;
    const passwordInput = getPasswordInput() as HTMLInputElement;

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'WrongPassword123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(passwordInput.value).toBe('');
      expect(usernameInput.value).toBe('testuser');
    });
  });

  it('should handle unexpected errors gracefully', async () => {
    mockLoginFn.mockRejectedValue(new Error('Network error'));

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/network error/i)).toBeInTheDocument();
    });
  });

  it('should show loading state during submission', async () => {
    mockLoginFn.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    expect(screen.getByText(/logging in/i)).toBeInTheDocument();
    expect(submitButton).toBeDisabled();
  });

  it('should disable inputs during loading', async () => {
    mockLoginFn.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    const passwordInput = getPasswordInput();

    fireEvent.change(usernameInput, { target: { value: 'testuser' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    expect(usernameInput).toBeDisabled();
    expect(passwordInput).toBeDisabled();
  });

  it('should call onForgotPassword when forgot password link is clicked', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const forgotPasswordLink = screen.getByText(/forgot password/i);
    fireEvent.click(forgotPasswordLink);

    expect(mockOnForgotPassword).toHaveBeenCalled();
  });

  it('should call onRegister when register link is clicked', () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const registerLink = screen.getByText(/register here/i);
    fireEvent.click(registerLink);

    expect(mockOnRegister).toHaveBeenCalled();
  });

  it('should clear field errors when user starts typing', async () => {
    renderWithProviders(
      <LoginForm 
        onLoginSuccess={mockOnLoginSuccess}
        onForgotPassword={mockOnForgotPassword}
        onRegister={mockOnRegister}
      />
    );

    const submitButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/username or email is required/i)).toBeInTheDocument();
    });

    const usernameInput = screen.getByRole('textbox', { name: /username or email/i });
    fireEvent.change(usernameInput, { target: { value: 't' } });

    await waitFor(() => {
      expect(screen.queryByText(/username or email is required/i)).not.toBeInTheDocument();
    });
  });
});

