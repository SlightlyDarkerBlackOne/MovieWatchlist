/**
 * Tests for ForgotPasswordForm component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '../../utils/test-utils';
import ForgotPasswordForm from './ForgotPasswordForm';
import { useAuth } from '../../contexts/AuthContext';

// Mock the useAuth hook
jest.mock('../../contexts/AuthContext', () => {
  const actual = jest.requireActual('../../contexts/AuthContext');
  return {
    ...actual,
    useAuth: jest.fn(),
  };
});

const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('ForgotPasswordForm', () => {
  const mockForgotPasswordFn = jest.fn();
  const mockOnBackToLogin = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseAuth.mockReturnValue({
      forgotPassword: mockForgotPasswordFn,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      resetPassword: jest.fn(),
      isAuthenticated: jest.fn(() => false),
      user: null,
      isLoading: false,
    });
  });

  it('should render forgot password form with email field', () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByRole('heading', { name: /forgot password/i })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /email/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument();
  });

  it('should show form description', () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByText(/enter your email address/i)).toBeInTheDocument();
  });

  it('should show back to login link', () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    expect(screen.getByText(/back to login/i)).toBeInTheDocument();
  });

  it('should update email field when typing', () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const emailInput = screen.getByRole('textbox', { name: /email/i }) as HTMLInputElement;
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });

    expect(emailInput.value).toBe('test@example.com');
  });

  it('should show validation error for empty email', async () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const submitButton = screen.getByRole('button', { name: /send reset link/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    });

    expect(mockForgotPasswordFn).not.toHaveBeenCalled();
  });

  it('should show validation error for invalid email format', async () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

    const backToLoginLink = screen.getByText(/back to login/i);
    fireEvent.click(backToLoginLink);

    expect(mockOnBackToLogin).toHaveBeenCalled();
  });

  it('should clear field error when user starts typing', async () => {
    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

    render(<ForgotPasswordForm onBackToLogin={mockOnBackToLogin} />);

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

