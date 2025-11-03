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
import { TestConstants } from '../__tests__/TestConstants';
import { WatchlistStatus } from '../types/watchlist.types';

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
jest.mock('../hooks/useWatchlistPresence', () => ({
  useWatchlistPresence: jest.fn(),
}));
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
const { useWatchlistPresence } = require('../hooks/useWatchlistPresence');
const mockedUseWatchlistPresence = useWatchlistPresence as jest.MockedFunction<typeof useWatchlistPresence>;

describe('MovieDetailsPage', () => {
  const mockTmdbId = String(TestConstants.Movies.DefaultTmdbId);

  const mockAuthContext = {
    user: mockUser,
  };

  const mockMovieData = {
    movie: mockMovieDetails,
    credits: mockMovieCredits,
    videos: [mockMovieVideo],
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockedUseParams.mockReturnValue({ tmdbId: mockTmdbId });
    
    // Reset and set up the mocks
    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockClear();
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockClear();
    (watchlistApi.useAddToWatchlistMutation as jest.Mock).mockClear();
    (watchlistApi.useRemoveFromWatchlistMutation as jest.Mock).mockClear();
    mockedUseWatchlistPresence.mockClear();

    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: mockMovieData,
      isLoading: false,
      isError: false,
      error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
    });

    // Mock useGetWatchlistQuery for MovieDetailsPage direct usage
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
          isLoading: false,
          isError: false,
      isFetching: false,
      isSuccess: true,
      isUninitialized: false,
          error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
      currentData: mockWatchlistItems,
    });

    // Mock useWatchlistPresence - simpler than mocking RTK Query internals
    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => {
      const isInWatchlist = mockWatchlistItems.some(item => item.movie?.tmdbId === tmdbId);
      return {
        isInWatchlist,
        isLoading: false,
      };
    });

    (watchlistApi.useAddToWatchlistMutation as jest.Mock).mockImplementation(() => [
      jest.fn().mockResolvedValue({ data: {} }),
      { isLoading: false, isError: false, error: undefined, status: 'idle' },
    ]);

    (watchlistApi.useRemoveFromWatchlistMutation as jest.Mock).mockImplementation(() => [
      jest.fn().mockResolvedValue({ data: {} }),
      { isLoading: false, isError: false, error: undefined, status: 'idle' },
    ]);

    (movieService.findMainTrailer as jest.Mock).mockReturnValue(mockMovieVideo);
  });

  it('should load movie details on mount using RTK Query', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(moviesApi.useGetMovieDetailsQuery).toHaveBeenCalled();
    });
    
    expect(moviesApi.useGetMovieDetailsQuery).toHaveBeenCalledWith(
      { tmdbId: parseInt(mockTmdbId) },
      expect.objectContaining({
        skip: false
      })
    );
  });

  it('should display movie title and overview', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });
  });

  it('should display genres', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      mockMovieDetails.genres.forEach(genre => {
        expect(screen.getByText(genre)).toBeInTheDocument();
      });
    });
  });

  it('should display top cast', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText('Top Cast')).toBeInTheDocument();
      expect(screen.getByText(mockMovieCredits.cast[0].name)).toBeInTheDocument();
    });
  });

  it('should show loading spinner while loading', () => {
    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: undefined,
      status: 'pending',
      refetch: jest.fn(),
    });

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('should display error message on load failure', async () => {
    const errorObj = { status: 500, data: { message: 'Failed to load movie' } };
    // Make String() conversion return the message
    Object.defineProperty(errorObj, 'toString', {
      value: () => 'Failed to load movie',
      enumerable: false,
    });
    
    (moviesApi.useGetMovieDetailsQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: errorObj,
      status: 'rejected',
      refetch: jest.fn(),
    });

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(/Failed to load movie/i)).toBeInTheDocument();
    });
  });

  it('should show/hide trailer on button click', async () => {
    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

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
    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: false,
      isLoading: false,
    }));

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const addButton = screen.getByRole('button', { name: /add to watchlist/i });
    fireEvent.click(addButton);

    await waitFor(() => {
      const dialog = screen.getByRole('dialog');
      expect(dialog).toBeInTheDocument();
      expect(dialog).toHaveTextContent('Add to Watchlist');
    });
  });

  it('should handle missing trailer gracefully', async () => {
    (movieService.findMainTrailer as jest.Mock).mockReturnValue(null);

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    expect(screen.queryByRole('button', { name: /play trailer/i })).not.toBeInTheDocument();
  });

  it('should show "In Watchlist" button when movie is already in watchlist', async () => {
    // Mock useGetWatchlistQuery to return watchlist with the movie
    const watchlistWithMovie = [
      ...mockWatchlistItems,
      {
        id: 999,
        userId: mockUser.id,
        movieId: 999,
        movie: {
          id: 999,
          tmdbId: parseInt(mockTmdbId),
          title: mockMovieDetails.title,
          overview: mockMovieDetails.overview,
          releaseDate: mockMovieDetails.releaseDate,
          posterPath: mockMovieDetails.posterPath,
          voteAverage: mockMovieDetails.voteAverage,
          voteCount: mockMovieDetails.voteCount,
          genreIds: mockMovieDetails.genreIds,
        },
        status: WatchlistStatus.Planned,
        isFavorite: false,
        addedDate: new Date().toISOString(),
      },
    ];
    
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: watchlistWithMovie,
      isLoading: false,
      isError: false,
      isFetching: false,
      isSuccess: true,
      isUninitialized: false,
      error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
      currentData: watchlistWithMovie,
    });

    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: tmdbId === parseInt(mockTmdbId),
      isLoading: false,
    }));

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(mockMovieDetails.title)).toBeInTheDocument();
    });

    const watchlistButton = screen.getByRole('button', { name: /in watchlist/i });
    expect(watchlistButton).toBeInTheDocument();
    expect(watchlistButton).not.toBeDisabled();
  });

  it('should show "Add to Watchlist" button enabled when movie is not in watchlist', async () => {
    // Mock useGetWatchlistQuery to return watchlist without this movie
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems, // Doesn't contain the movie being viewed
      isLoading: false,
      isError: false,
      isFetching: false,
      isSuccess: true,
      isUninitialized: false,
      error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
      currentData: mockWatchlistItems,
    });

    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: false,
      isLoading: false,
    }));

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

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
      { isLoading: false, isError: false, error: undefined, status: 'idle' },
    ]);

    // Mock useGetWatchlistQuery to return watchlist with the movie
    const watchlistWithMovie = [
      ...mockWatchlistItems,
      {
        id: 999,
        userId: mockUser.id,
        movieId: 999,
        movie: {
          id: 999,
          tmdbId: parseInt(mockTmdbId),
          title: mockMovieDetails.title,
          overview: mockMovieDetails.overview,
          releaseDate: mockMovieDetails.releaseDate,
          posterPath: mockMovieDetails.posterPath,
          voteAverage: mockMovieDetails.voteAverage,
          voteCount: mockMovieDetails.voteCount,
          genreIds: mockMovieDetails.genreIds,
        },
        status: WatchlistStatus.Planned,
        isFavorite: false,
        addedDate: new Date().toISOString(),
      },
    ];
    
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: watchlistWithMovie,
      isLoading: false,
      isError: false,
      isFetching: false,
      isSuccess: true,
      isUninitialized: false,
      error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
      currentData: watchlistWithMovie,
    });

    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: tmdbId === parseInt(mockTmdbId),
      isLoading: false,
    }));

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

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
    // Mock useGetWatchlistQuery to return watchlist with the movie
    const watchlistWithMovie = [
      ...mockWatchlistItems,
      {
        id: 999,
        userId: mockUser.id,
        movieId: 999,
        movie: {
          id: 999,
          tmdbId: parseInt(mockTmdbId),
          title: mockMovieDetails.title,
          overview: mockMovieDetails.overview,
          releaseDate: mockMovieDetails.releaseDate,
          posterPath: mockMovieDetails.posterPath,
          voteAverage: mockMovieDetails.voteAverage,
          voteCount: mockMovieDetails.voteCount,
          genreIds: mockMovieDetails.genreIds,
        },
        status: WatchlistStatus.Planned,
        isFavorite: false,
        addedDate: new Date().toISOString(),
      },
    ];
    
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: watchlistWithMovie,
      isLoading: false,
      isError: false,
      isFetching: false,
      isSuccess: true,
      isUninitialized: false,
      error: undefined,
      status: 'fulfilled',
      refetch: jest.fn(),
      currentData: watchlistWithMovie,
    });

    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: tmdbId === parseInt(mockTmdbId),
      isLoading: false,
    }));

    renderWithMocks(<MovieDetailsPage />, { mockAuthContext });

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


