/**
 * Tests for WatchlistItemCard component
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter } from 'react-router-dom';
import WatchlistItemCard from './WatchlistItemCard';
import { mockWatchlistItem, mockWatchlistItemWatched } from '../../__tests__/fixtures/watchlistFixtures';
import { useNavigate } from 'react-router-dom';
import { appTheme } from '../../theme';

// Mock navigation
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: jest.fn(),
}));

const mockedUseNavigate = useNavigate as jest.MockedFunction<typeof useNavigate>;

describe('WatchlistItemCard', () => {
  const mockNavigate = jest.fn();
  const mockOnUpdate = jest.fn();
  const mockOnDelete = jest.fn();

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
    mockedUseNavigate.mockReturnValue(mockNavigate);
    // Mock window.confirm
    global.confirm = jest.fn(() => true);
  });

  it('should render watchlist item correctly', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getByText(mockWatchlistItem.movie!.title)).toBeInTheDocument();
    expect(screen.getByText('1999')).toBeInTheDocument(); // Release year
  });

  it('should display status chip with correct label', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getByText('Planned')).toBeInTheDocument();
  });

  it('should display TMDB rating', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getByText(mockWatchlistItem.movie!.voteAverage.toFixed(1))).toBeInTheDocument();
    expect(screen.getByText('/10')).toBeInTheDocument();
  });

  it('should navigate to details on card click', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    // Click on the card content area (not on the buttons)
    const cardTitle = screen.getByText(mockWatchlistItem.movie!.title);
    fireEvent.click(cardTitle);

    expect(mockNavigate).toHaveBeenCalledWith(`/movies/${mockWatchlistItem.movie!.tmdbId}`);
  });

  it('should open menu on 3-dot icon click', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    const menuButton = screen.getAllByRole('button')[0]; // First button is menu
    fireEvent.click(menuButton);

    expect(screen.getByText('Edit')).toBeInTheDocument();
    expect(screen.getByText('Remove')).toBeInTheDocument();
  });

  it('should delete item with confirmation', async () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    // Open menu
    const menuButton = screen.getAllByRole('button')[0];
    fireEvent.click(menuButton);

    // Click delete
    const deleteButton = screen.getByText('Remove');
    fireEvent.click(deleteButton);

    await waitFor(() => {
      expect(global.confirm).toHaveBeenCalled();
      expect(mockOnDelete).toHaveBeenCalledWith(mockWatchlistItem.id);
    });
  });

  it('should not delete if user cancels confirmation', async () => {
    global.confirm = jest.fn(() => false);
    
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    // Open menu
    const menuButton = screen.getAllByRole('button')[0];
    fireEvent.click(menuButton);

    // Click delete
    const deleteButton = screen.getByText('Remove');
    fireEvent.click(deleteButton);

    await waitFor(() => {
      expect(global.confirm).toHaveBeenCalled();
      expect(mockOnDelete).not.toHaveBeenCalled();
    });
  });

  it('should toggle favorite on heart icon click', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    // Find favorite button (heart icon)
    const favoriteButton = screen.getByRole('button', { name: /add to favorites/i });
    fireEvent.click(favoriteButton);

    expect(mockOnUpdate).toHaveBeenCalledWith({
      ...mockWatchlistItem,
      isFavorite: true,
    });
    expect(mockNavigate).not.toHaveBeenCalled(); // Should not navigate
  });

  it('should show filled heart for favorited items', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItemWatched} // This item is favorited
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getByRole('button', { name: /remove from favorites/i })).toBeInTheDocument();
  });

  it('should prevent navigation when clicking menu', () => {
    renderWithProviders(
      <WatchlistItemCard 
        item={mockWatchlistItem}
        onUpdate={mockOnUpdate}
        onDelete={mockOnDelete}
      />
    );

    const menuButton = screen.getAllByRole('button')[0];
    fireEvent.click(menuButton);

    // Should not navigate when opening menu
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});


