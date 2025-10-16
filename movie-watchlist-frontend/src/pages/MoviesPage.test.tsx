/**
 * Tests for MoviesPage component
 */

import React from 'react';
import { screen, waitFor, act } from '@testing-library/react';
import { render } from '../utils/test-utils';
import MoviesPage from './MoviesPage';
import movieService from '../services/movieService';
import { mockMovies } from '../__tests__/fixtures/movieFixtures';
import { useSearchParams } from 'react-router-dom';

// Mock services
jest.mock('../services/movieService');
jest.mock('../contexts/WatchlistContext', () => ({
  ...jest.requireActual('../contexts/WatchlistContext'),
  useWatchlist: () => ({
    addToWatchlist: jest.fn(),
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
  }),
}));
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useSearchParams: jest.fn(),
}));

const mockedMovieService = movieService as jest.Mocked<typeof movieService>;
const mockedUseSearchParams = useSearchParams as jest.MockedFunction<typeof useSearchParams>;

describe('MoviesPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedMovieService.getPopularMovies.mockResolvedValue({
      movies: mockMovies,
      totalResults: 3,
      totalPages: 1,
      currentPage: 1,
    });
    mockedMovieService.clearPopularMoviesCache = jest.fn();
    mockedUseSearchParams.mockReturnValue([new URLSearchParams(), jest.fn()]);
  });

  it('should load popular movies on mount', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockedMovieService.getPopularMovies).toHaveBeenCalledWith(1);
    });
  });

  it('should render featured carousel with popular movies', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      // Carousel should be rendered with first movie title
      // Note: Title appears in both carousel and popular movies section
      const titles = screen.getAllByText(mockMovies[0].title);
      expect(titles.length).toBeGreaterThan(0);
      expect(titles[0]).toBeInTheDocument();
    });
  });

  it('should display popular movies section', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      expect(screen.getByText('Popular Movies')).toBeInTheDocument();
    });
  });

  it('should show refresh button for popular movies', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      const refreshButton = screen.getByRole('button', { name: /refresh popular movies/i });
      expect(refreshButton).toBeInTheDocument();
    });
  });

  it('should handle search from URL params', async () => {
    const searchQuery = 'fight club';
    mockedMovieService.searchMovies.mockResolvedValue({
      movies: [mockMovies[0]],
      totalResults: 1,
      totalPages: 1,
      currentPage: 1,
    });

    // Mock useSearchParams to return search query
    mockedUseSearchParams.mockReturnValue([new URLSearchParams({ search: searchQuery }), jest.fn()]);

    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockedMovieService.searchMovies).toHaveBeenCalledWith(searchQuery, 1);
    });
  });

  it('should display error message on API failure', async () => {
    mockedMovieService.getPopularMovies.mockRejectedValue(
      new Error('Failed to fetch movies')
    );

    render(<MoviesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed/i)).toBeInTheDocument();
    });
  });

  it('should auto-refresh popular movies every 3 minutes', async () => {
    jest.useFakeTimers();

    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockedMovieService.getPopularMovies).toHaveBeenCalledTimes(1);
    });

    // Fast-forward 3 minutes
    act(() => {
      jest.advanceTimersByTime(3 * 60 * 1000);
    });

    await waitFor(() => {
      expect(mockedMovieService.getPopularMovies).toHaveBeenCalledTimes(2);
      expect(mockedMovieService.clearPopularMoviesCache).toHaveBeenCalled();
    });

    jest.useRealTimers();
  });
});


