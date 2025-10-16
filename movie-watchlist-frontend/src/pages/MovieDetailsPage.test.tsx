/**
 * Tests for MovieDetailsPage component
 */

import React from 'react';
import { screen, waitFor, fireEvent } from '@testing-library/react';
import { renderWithMocks } from '../utils/test-utils';
import MovieDetailsPage from './MovieDetailsPage';
import movieService from '../services/movieService';
import { mockMovieDetails, mockMovieCredits, mockMovieVideo } from '../__tests__/fixtures/movieFixtures';
import { useParams } from 'react-router-dom';

// Mock dependencies
jest.mock('../services/movieService');
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useParams: jest.fn(),
  useNavigate: () => jest.fn(),
}));

const mockedMovieService = movieService as jest.Mocked<typeof movieService>;
const mockedUseParams = useParams as jest.MockedFunction<typeof useParams>;

describe('MovieDetailsPage', () => {
  const mockTmdbId = '550';

  // Mock watchlist context for all tests
  const mockWatchlistContext = {
    addToWatchlist: jest.fn(),
    removeFromWatchlist: jest.fn(),
    isInWatchlist: jest.fn(() => false),
    watchlistMovieIds: [],
    selectedMovie: null,
    addDialogOpen: false,
    status: 0,
    notes: '',
    successMessage: null,
    error: null,
    setStatus: jest.fn(),
    setNotes: jest.fn(),
    handleCloseDialog: jest.fn(),
    handleConfirmAdd: jest.fn(),
    refreshWatchlistIds: jest.fn(),
    removeFromWatchlistIds: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseParams.mockReturnValue({ tmdbId: mockTmdbId });
    mockedMovieService.getMovieDetailsByTmdbId.mockResolvedValue({
      movie: mockMovieDetails,
      credits: mockMovieCredits,
      videos: [mockMovieVideo],
    });
    mockedMovieService.findMainTrailer.mockReturnValue(mockMovieVideo);
    mockedMovieService.getPosterUrl.mockReturnValue('https://image.tmdb.org/poster.jpg');
    mockedMovieService.getBackdropUrl.mockReturnValue('https://image.tmdb.org/backdrop.jpg');
  });

  it('should load movie details on mount', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(mockedMovieService.getMovieDetailsByTmdbId).toHaveBeenCalledWith(parseInt(mockTmdbId));
    });
  });

  it('should display movie title and overview', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });
  });

  it('should display genres', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      mockMovieDetails.genres.forEach(genre => {
        expect(screen.getByText(genre)).toBeInTheDocument();
      });
    });
  });

  it('should display top cast', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText('Top Cast')).toBeInTheDocument();
      expect(screen.getByText(mockMovieCredits.cast[0].name)).toBeInTheDocument();
    });
  });

  it('should show loading spinner while loading', () => {
    mockedMovieService.getMovieDetailsByTmdbId.mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('should display error message on load failure', async () => {
    mockedMovieService.getMovieDetailsByTmdbId.mockRejectedValue(
      new Error('Failed to load movie')
    );

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText('Failed to load movie')).toBeInTheDocument();
    });
  });

  it('should show/hide trailer on button click', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    // Initially, trailer should not be visible
    expect(screen.queryByTitle(/youtube/i)).not.toBeInTheDocument();

    // Click play trailer button
    const playButton = screen.getByRole('button', { name: /play trailer/i });
    fireEvent.click(playButton);

    // Trailer should now be visible
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /hide trailer/i })).toBeInTheDocument();
    });
  });

  it('should open add to watchlist dialog on button click', async () => {
    const mockAddToWatchlist = jest.fn();
    
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      addToWatchlist: mockAddToWatchlist,
    };

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const addButton = screen.getByRole('button', { name: /add to watchlist/i });
    fireEvent.click(addButton);

    // Dialog should open (handled by context)
    expect(mockAddToWatchlist).toHaveBeenCalled();
  });

  it('should handle missing trailer gracefully', async () => {
    mockedMovieService.findMainTrailer.mockReturnValue(null);

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    // Play trailer button should not be visible
    expect(screen.queryByRole('button', { name: /play trailer/i })).not.toBeInTheDocument();
  });

  it('should show "In Watchlist" button when movie is already in watchlist', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: [mockMovieDetails.tmdbId],
    };

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    expect(watchlistButton).toBeInTheDocument();
    expect(watchlistButton).not.toBeDisabled(); // Button should be clickable to remove from watchlist
  });

  it('should show "Add to Watchlist" button enabled when movie is not in watchlist', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => false),
      watchlistMovieIds: [],
    };

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /add to watchlist/i });
    expect(watchlistButton).toBeInTheDocument();
    expect(watchlistButton).not.toBeDisabled();
  });

  it('should call removeFromWatchlist when clicking "In Watchlist" button', async () => {
    const mockRemoveFromWatchlist = jest.fn();
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      removeFromWatchlist: mockRemoveFromWatchlist,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: [mockMovieDetails.tmdbId],
    };

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    fireEvent.click(watchlistButton);

    expect(mockRemoveFromWatchlist).toHaveBeenCalledWith(mockMovieDetails.tmdbId);
  });

  it('should show "Remove from Watchlist" text when hovering over "In Watchlist" button', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: [mockMovieDetails.tmdbId],
    };

    renderWithMocks(<MovieDetailsPage />, { mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    expect(watchlistButton).toBeInTheDocument();

    // Hover over the button
    fireEvent.mouseEnter(watchlistButton);

    // Should now show "Remove from Watchlist"
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /remove from watchlist/i })).toBeInTheDocument();
    });

    // Mouse leave
    fireEvent.mouseLeave(watchlistButton);

    // Should go back to "In Watchlist"
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /in watchlist/i })).toBeInTheDocument();
    });
  });
});


