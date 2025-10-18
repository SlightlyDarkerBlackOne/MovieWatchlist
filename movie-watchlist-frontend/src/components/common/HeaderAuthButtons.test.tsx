/**
 * Tests for HeaderAuthButtons component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import HeaderAuthButtons from './HeaderAuthButtons';

// Mock AuthContext
const mockLogout = jest.fn();
const mockIsAuthenticated = jest.fn();

const mockNavigate = jest.fn();

jest.mock('../../contexts/AuthContext', () => ({
  useAuth: () => ({
    logout: mockLogout,
    isAuthenticated: mockIsAuthenticated,
  }),
}));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('HeaderAuthButtons', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockNavigate.mockClear();
  });

  it('should always show My Watchlist button', () => {
    mockIsAuthenticated.mockReturnValue(false);
    renderWithRouter(<HeaderAuthButtons />);
    
    expect(screen.getByRole('button', { name: /my watchlist/i })).toBeInTheDocument();
  });

  it('should show Login button when not authenticated', () => {
    mockIsAuthenticated.mockReturnValue(false);
    renderWithRouter(<HeaderAuthButtons />);
    
    expect(screen.getByRole('button', { name: /^login$/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /logout/i })).not.toBeInTheDocument();
  });

  it('should show Logout button when authenticated', () => {
    mockIsAuthenticated.mockReturnValue(true);
    renderWithRouter(<HeaderAuthButtons />);
    
    expect(screen.getByRole('button', { name: /logout/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^login$/i })).not.toBeInTheDocument();
  });

  it('should navigate to watchlist page when clicking My Watchlist', () => {
    mockIsAuthenticated.mockReturnValue(true);
    renderWithRouter(<HeaderAuthButtons />);
    
    const watchlistButton = screen.getByRole('button', { name: /my watchlist/i });
    fireEvent.click(watchlistButton);
    
    expect(mockNavigate).toHaveBeenCalledWith('/watchlist');
  });

  it('should navigate to login page when clicking Login button', () => {
    mockIsAuthenticated.mockReturnValue(false);
    renderWithRouter(<HeaderAuthButtons />);
    
    const loginButton = screen.getByRole('button', { name: /^login$/i });
    fireEvent.click(loginButton);
    
    expect(mockNavigate).toHaveBeenCalledWith('/login');
  });

  it('should call logout and redirect when clicking Logout button', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockLogout.mockResolvedValue(true);
    
    renderWithRouter(<HeaderAuthButtons />);
    
    const logoutButton = screen.getByRole('button', { name: /logout/i });
    fireEvent.click(logoutButton);
    
    await waitFor(() => {
      expect(mockLogout).toHaveBeenCalled();
    });
  });

  it('should have proper button styling with gap', () => {
    mockIsAuthenticated.mockReturnValue(true);
    const { container } = renderWithRouter(<HeaderAuthButtons />);
    
    const buttonContainer = container.firstChild;
    expect(buttonContainer).toHaveStyle({ display: 'flex', alignItems: 'center' });
  });
});

