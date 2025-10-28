/**
 * Tests for RegisterForm component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '../../utils/test-utils';
import RegisterForm from './RegisterForm';
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

describe('RegisterForm', () => {
  const mockRegisterFn = jest.fn();
  const mockOnRegisterSuccess = jest.fn();
  const mockOnBackToLogin = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseAuth.mockReturnValue({
      register: mockRegisterFn,
      login: jest.fn(),
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

  it('should render register form with all fields', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    expect(screen.getByRole('heading', { name: /register/i })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /^username$/i })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: /email/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    const inputs = screen.getAllByLabelText(/confirm password/i);
    expect(inputs[0]).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /register/i })).toBeInTheDocument();
  });

  it('should show back to login link', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    expect(screen.getByText(/already have an account/i)).toBeInTheDocument();
    expect(screen.getByText(/back to login/i)).toBeInTheDocument();
  });

  it('should update input fields when typing', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i }) as HTMLInputElement;
    const emailInput = screen.getByRole('textbox', { name: /email/i }) as HTMLInputElement;
    const passwordInput = screen.getByLabelText(/^password$/i) as HTMLInputElement;
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0] as HTMLInputElement;

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'newuser@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'NewPassword123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'NewPassword123!' } });

    expect(usernameInput.value).toBe('newuser');
    expect(emailInput.value).toBe('newuser@example.com');
    expect(passwordInput.value).toBe('NewPassword123!');
    expect(confirmPasswordInput.value).toBe('NewPassword123!');
  });

  it('should toggle password visibility for both password fields', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const passwordInput = screen.getByLabelText(/^password$/i) as HTMLInputElement;
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0] as HTMLInputElement;
    const toggleButtons = screen.getAllByLabelText(/toggle.*password visibility/i);

    expect(passwordInput.type).toBe('password');
    expect(confirmPasswordInput.type).toBe('password');

    // Toggle main password
    fireEvent.click(toggleButtons[0]);
    expect(passwordInput.type).toBe('text');

    // Toggle confirm password
    fireEvent.click(toggleButtons[1]);
    expect(confirmPasswordInput.type).toBe('text');
  });

  it('should show validation errors for empty fields', async () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/username is required/i)).toBeInTheDocument();
      expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    });

    expect(mockRegisterFn).not.toHaveBeenCalled();
  });

  it('should show validation error for invalid email', async () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'Password123' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/invalid email/i)).toBeInTheDocument();
    });

    expect(mockRegisterFn).not.toHaveBeenCalled();
  });

  it('should show validation error when passwords do not match', async () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'newuser@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'DifferentPassword123!' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
    });

    expect(mockRegisterFn).not.toHaveBeenCalled();
  });

  it('should submit form with valid data and call onRegisterSuccess', async () => {
    mockRegisterFn.mockResolvedValue({
      isSuccess: true,
      user: { id: 1, username: 'newuser', email: 'newuser@example.com' },
    });

    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'newuser@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'Password123!' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockRegisterFn).toHaveBeenCalledWith({
        username: 'newuser',
        email: 'newuser@example.com',
        password: 'Password123!',
      });
      expect(mockOnRegisterSuccess).toHaveBeenCalled();
    });
  });

  it('should show error message on registration failure', async () => {
    const errorMessage = 'Username already exists';
    mockRegisterFn.mockResolvedValue({
      isSuccess: false,
      errorMessage,
    });

    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'existinguser' } });
    fireEvent.change(emailInput, { target: { value: 'existing@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'Password123!' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });

    expect(mockOnRegisterSuccess).not.toHaveBeenCalled();
  });

  it('should handle unexpected errors gracefully', async () => {
    mockRegisterFn.mockRejectedValue(new Error('Network error'));

    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'newuser@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'Password123!' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/unexpected error/i)).toBeInTheDocument();
    });
  });

  it('should show loading state during submission', async () => {
    mockRegisterFn.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));

    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    const emailInput = screen.getByRole('textbox', { name: /email/i });
    const passwordInput = screen.getByLabelText(/^password$/i);
    const confirmPasswordInputs = screen.getAllByLabelText(/confirm password/i);
    const confirmPasswordInput = confirmPasswordInputs[0];

    fireEvent.change(usernameInput, { target: { value: 'newuser' } });
    fireEvent.change(emailInput, { target: { value: 'newuser@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'Password123!' } });
    fireEvent.change(confirmPasswordInput, { target: { value: 'Password123!' } });

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    expect(screen.getByText(/registering/i)).toBeInTheDocument();
    expect(submitButton).toBeDisabled();
  });

  it('should call onBackToLogin when back to login link is clicked', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const backToLoginLink = screen.getByText(/back to login/i);
    fireEvent.click(backToLoginLink);

    expect(mockOnBackToLogin).toHaveBeenCalled();
  });

  it('should clear field errors when user starts typing', async () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    const submitButton = screen.getByRole('button', { name: /register/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/username is required/i)).toBeInTheDocument();
    });

    const usernameInput = screen.getByRole('textbox', { name: /^username$/i });
    fireEvent.change(usernameInput, { target: { value: 'n' } });

    await waitFor(() => {
      expect(screen.queryByText(/username is required/i)).not.toBeInTheDocument();
    });
  });

  it('should show password requirements helper text', () => {
    render(
      <RegisterForm 
        onRegisterSuccess={mockOnRegisterSuccess}
        onBackToLogin={mockOnBackToLogin}
      />
    );

    expect(screen.getByText(/must be at least 8 characters/i)).toBeInTheDocument();
  });
});

