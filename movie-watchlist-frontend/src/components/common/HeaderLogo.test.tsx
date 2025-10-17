/**
 * Tests for HeaderLogo component
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import HeaderLogo from './HeaderLogo';

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('HeaderLogo', () => {
  it('should render logo icon and title', () => {
    renderWithRouter(<HeaderLogo />);
    
    expect(screen.getByRole('button', { name: /movie watchlist home/i })).toBeInTheDocument();
    expect(screen.getByText('MovieWatchlist')).toBeInTheDocument();
  });

  it('should have proper aria label for accessibility', () => {
    renderWithRouter(<HeaderLogo />);
    
    const iconButton = screen.getByRole('button', { name: /movie watchlist home/i });
    expect(iconButton).toHaveAttribute('aria-label', 'movie watchlist home');
  });

  it('should navigate to home when clicking icon', () => {
    renderWithRouter(<HeaderLogo />);
    
    const iconButton = screen.getByRole('button', { name: /movie watchlist home/i });
    fireEvent.click(iconButton);
    
    // Should navigate to home (verify by checking window location)
    expect(window.location.pathname).toBe('/');
  });

  it('should navigate to home when clicking title', () => {
    renderWithRouter(<HeaderLogo />);
    
    const title = screen.getByText('MovieWatchlist');
    fireEvent.click(title);
    
    // Should navigate to home
    expect(window.location.pathname).toBe('/');
  });

  it('should have cursor pointer style on title', () => {
    renderWithRouter(<HeaderLogo />);
    
    const title = screen.getByText('MovieWatchlist');
    expect(title).toHaveStyle({ cursor: 'pointer' });
  });
});

