/**
 * Tests for ForgotPasswordForm component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter } from 'react-router-dom';
import ForgotPasswordForm from './ForgotPasswordForm';
import { appTheme } from '../../theme';
import { useAuth } from '../../contexts/AuthContext';

// Mock the useAuth hook
jest.mock('../../contexts/AuthContext', () => ({
  useAuth: jest.fn(),
}));

const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('ForgotPasswordForm', () => {
  const mockForgotPasswordFn = jest.fn();
  const mockOnBackToLogin = jest.fn();

  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <ThemeProvider theme={appTheme}>
        <BrowserRouter>
          {ui}
        </BrowserRouter>
      </ThemeProvider>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseAuth.mockReturnValue({
      forgotPassword: mockForgotPasswordFn,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      resetPassword: jest.fn(),
      validateToken: jest.fn(),
      isAuthenticated: jest.fn(() => false),
      getToken: jest.fn(() => null),
      user: null,
      isLoading: false,
    });
  });

  it('should render forgot password form with email field', () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByRole('heading', { name: /forgot password/i })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /email/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument();
  });

  it('should show form description', () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByText(/enter your email address/i)).toBeInTheDocument();
  });

  it('should show back to login link', () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByText(/back to login/i)).toBeInTheDocument();
  });

  it('should update email field when typing', () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i }) as HTMLInputElement;
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    expect(emailInput.value).toBe('test@example.com');
  });

  it('should show validation error for empty email', async () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    });

    expect(mockForgotPasswordFn).not.toHaveBeenCalled();
  });

  it('should show validation error for invalid email format', async () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid email/i)).toBeInTheDocument();
    });

    expect(mockForgotPasswordFn).not.toHaveBeenCalled();
  });

  it('should clear email field after successful submission', async () => {
    mockForgotPasswordFn.mockResolvedValue({
      success: true,
      message: 'Password reset email sent successfully',
    });

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i }) as HTMLInputElement;
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(emailInput.value).toBe('');
    });
  });

  it('should show error message on failure', async () => {
    const errorMessage = 'Email not found';
    mockForgotPasswordFn.mockResolvedValue({
      success: false,
      message: errorMessage,
    });

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'nonexistent@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it('should handle unexpected errors gracefully', async () => {
    mockForgotPasswordFn.mockRejectedValue(new Error('Network error'));

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/network error/i)).toBeInTheDocument();
    }, { timeout: 3000 });
  });

  it('should show loading state during submission', async () => {
    mockForgotPasswordFn.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    expect(screen.getByText(/sending/i)).toBeInTheDocument();
    expect(submitButton).toBeDisabled();
    expect(emailInput).toBeDisabled();
  });

  it('should disable form after successful submission', async () => {
    mockForgotPasswordFn.mockResolvedValue({
      success: true,
      message: 'Password reset email sent successfully',
    });

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(emailInput).toBeDisabled();
      expect(submitButton).toBeDisabled();
    });
  });

  it('should call onBackToLogin when back to login link is clicked', () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const backToLoginLink = screen.getByText(/back to login/i);
    fireEvent.click(backToLoginLink);

    expect(mockOnBackToLogin).toHaveBeenCalled();
  });

  it('should clear field error when user starts typing', async () => {
    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    });

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 't' } });

    await waitFor(() => {
      expect(screen.queryByText(/email is required/i)).not.toBeInTheDocument();
    });
  });

  it('should clear general error when user starts typing', async () => {
    mockForgotPasswordFn.mockResolvedValue({
      success: false,
      message: 'Email not found',
    });

    renderWithProviders(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i });
    fireEvent.change(emailInput, { target: { value: 'nonexistent@example.com' } });

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email not found/i)).toBeInTheDocument();
    });

    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    await waitFor(() => {
      expect(screen.queryByText(/email not found/i)).not.toBeInTheDocument();
    });
  });
});

