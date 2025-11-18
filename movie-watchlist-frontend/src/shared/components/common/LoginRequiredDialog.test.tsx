/**
 * Tests for LoginRequiredDialog component
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginRequiredDialog from './LoginRequiredDialog';

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('LoginRequiredDialog', () => {
  const mockOnClose = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render when open is true', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    expect(screen.getByText('Login Required')).toBeInTheDocument();
    expect(screen.getByText(/please log in to add movies to your watchlist/i)).toBeInTheDocument();
  });

  it('should not render when open is false', () => {
    renderWithRouter(<LoginRequiredDialog open={false} onClose={mockOnClose} />);
    
    expect(screen.queryByText('Login Required')).not.toBeInTheDocument();
  });

  it('should show Cancel and Go to Login buttons', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go to login/i })).toBeInTheDocument();
  });

  it('should call onClose when clicking Cancel button', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    fireEvent.click(cancelButton);
    
    expect(mockOnClose).toHaveBeenCalledTimes(1);
  });

  it('should call onClose and navigate to login when clicking Go to Login', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    const loginButton = screen.getByRole('button', { name: /go to login/i });
    fireEvent.click(loginButton);
    
    expect(mockOnClose).toHaveBeenCalledTimes(1);
    expect(window.location.pathname).toBe('/login');
  });

  it('should call onClose when clicking backdrop', () => {
    const { baseElement } = renderWithRouter(
      <LoginRequiredDialog open={true} onClose={mockOnClose} />
    );
    
    // Find the backdrop (MUI creates it as a sibling to the dialog)
    const backdrop = baseElement.querySelector('.MuiBackdrop-root');
    if (backdrop) {
      fireEvent.click(backdrop);
      expect(mockOnClose).toHaveBeenCalled();
    }
  });

  it('should display full error message', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    expect(screen.getByText('Please log in to add movies to your watchlist.')).toBeInTheDocument();
  });

  it('should have Go to Login button as primary variant', () => {
    renderWithRouter(<LoginRequiredDialog open={true} onClose={mockOnClose} />);
    
    const loginButton = screen.getByRole('button', { name: /go to login/i });
    expect(loginButton).toHaveClass('MuiButton-contained');
    expect(loginButton).toHaveClass('MuiButton-colorPrimary');
  });
});

