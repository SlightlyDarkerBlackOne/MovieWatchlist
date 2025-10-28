/**
 * Tests for MovieDetailsPage component
 * 
 * NOTE: Updated to use RTK Query hooks instead of service layer
 */

import React from 'react';
import { screen, waitFor, fireEvent } from '@testing-library/react';
import { renderWithMocks } from '../utils/test-utils';
import MovieDetailsPage from './MovieDetailsPage';
import * as moviesApi from '../store/api/moviesApi';
import * as watchlistApi from '../store/api/watchlistApi';
import * as movieService from '../services/movieService';
import { mockMovieDetails, mockMovieCredits, mockMovieVideo } from '../__tests__/fixtures/movieFixtures';
import { mockWatchlistItems } from '../__tests__/fixtures/watchlistFixtures';
import { mockUser } from '../__tests__/fixtures/authFixtures';
import { useParams } from 'react-router-dom';

jest.mock('../store/api/moviesApi', () => {
  const actual = jest.requireActual('../store/api/moviesApi');
  return {
    ...actual,
    useGetMovieDetailsQuery: jest.fn(),
  };
});

jest.mock('../store/api/watchlistApi', () => {
  const actual = jest.requireActual('../store/api/watchlistApi');
  return {
    ...actual,
    useGetWatchlistQuery: jest.fn(),
    useAddToWatchlistMutation: jest.fn(),
    useRemoveFromWatchlistMutation: jest.fn(),
  };
});
jest.mock('../services/movieService', () => ({
  ...jest.requireActual('../services/movieService'),
  findMainTrailer: jest.fn(),
}));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useParams: jest.fn(),
  useNavigate: () => jest.fn(),
}));

const mockedUseParams = useParams as jest.MockedFunction<typeof useParams>;

describe('MovieDetailsPage', () => {
  const mockTmdbId = '550';

  const mockAuthContext = {
    user: mockUser,
  };

  const mockWatchlistContext = {
    watchlistMovieIds: new Set<number>(),
    isInWatchlist: jest.fn(() => false),
  };

  const mockMovieData = {
    movie: mockMovieDetails,
    credits: mockMovieCredits,
    videos: [mockMovieVideo],
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseParams.mockReturnValue({ tmdbId: mockTmdbId });

    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: mockMovieData,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    });

    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      error: undefined,
    });

    (watchlistApi.useAddToWatchlistMutation as jest.Mock).mockReturnValue([
      jest.fn().mockResolvedValue({ data: {} }),
      { isLoading: false, error: undefined },
    ]);

    (watchlistApi.useRemoveFromWatchlistMutation as jest.Mock).mockReturnValue([
      jest.fn().mockResolvedValue({ data: {} }),
      { isLoading: false, error: undefined },
    ]);

    (movieService.findMainTrailer as jest.Mock).mockReturnValue(mockMovieVideo);
  });

  it('should load movie details on mount using RTK Query', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      expect(moviesApi.useGetMovieDetailsQuery).toHaveBeenCalledWith(
        { tmdbId: parseInt(mockTmdbId) },
        { skip: false }
      );
    });
  });

  it('should display movie title and overview', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });
  });

  it('should display genres', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      mockMovieDetails.genres.forEach(genre => {
        expect(screen.getByText(genre)).toBeInTheDocument();
      });
    });
  });

  it('should display top cast', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText('Top Cast')).toBeInTheDocument();
      expect(screen.getByText(mockMovieCredits.cast[0].name)).toBeInTheDocument();
    });
  });

  it('should show loading spinner while loading', () => {
    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      error: undefined,
      refetch: jest.fn(),
    });

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('should display error message on load failure', async () => {
    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: 'Failed to load movie',
      refetch: jest.fn(),
    });

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(/Failed to load movie/i)).toBeInTheDocument();
    });
  });

  it('should show/hide trailer on button click', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

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

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const addButton = screen.getByRole('button', { name: /add to watchlist/i });
    fireEvent.click(addButton);

    // Dialog should open (handled by context)
    expect(mockAddToWatchlist).toHaveBeenCalled();
  });

  it('should handle missing trailer gracefully', async () => {
    (movieService.findMainTrailer as jest.Mock).mockReturnValue(null);

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    expect(screen.queryByRole('button', { name: /play trailer/i })).not.toBeInTheDocument();
  });

  it('should show "In Watchlist" button when movie is already in watchlist', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: new Set([mockMovieDetails.tmdbId]),
    };

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    expect(watchlistButton).toBeInTheDocument();
    expect(watchlistButton).not.toBeDisabled();
  });

  it('should show "Add to Watchlist" button enabled when movie is not in watchlist', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => false),
      watchlistMovieIds: new Set<number>(),
    };

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /add to watchlist/i });
    expect(watchlistButton).toBeInTheDocument();
    expect(watchlistButton).not.toBeDisabled();
  });

  it('should call removeFromWatchlist when clicking "In Watchlist" button', async () => {
    const mockRemoveMutation = jest.fn().mockResolvedValue({ data: {} });
    (watchlistApi.useRemoveFromWatchlistMutation as jest.Mock).mockReturnValue([
      mockRemoveMutation,
      { isLoading: false },
    ]);

    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: new Set([mockMovieDetails.tmdbId]),
    };

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    fireEvent.click(watchlistButton);

    await waitFor(() => {
      expect(mockRemoveMutation).toHaveBeenCalled();
    });
  });

  it('should show "Remove from Watchlist" text when hovering over "In Watchlist" button', async () => {
    const customMockWatchlistContext = {
      ...mockWatchlistContext,
      isInWatchlist: jest.fn(() => true),
      watchlistMovieIds: new Set([mockMovieDetails.tmdbId]),
    };

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext, mockWatchlistContext: customMockWatchlistContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    expect(watchlistButton).toBeInTheDocument();

    fireEvent.mouseEnter(watchlistButton);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /remove from watchlist/i })).toBeInTheDocument();
    });

    fireEvent.mouseLeave(watchlistButton);

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /in watchlist/i })).toBeInTheDocument();
    });
  });
});


