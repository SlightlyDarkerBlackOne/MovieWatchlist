/**
 * Tests for MoviesPage component
 */

import React from 'react';
import { screen, waitFor, act } from '@testing-library/react';
import { render } from '../utils/test-utils';
import MoviesPage from './MoviesPage';
import { mockMovies } from '../__tests__/fixtures/movieFixtures';
import { useSearchParams } from 'react-router-dom';
import * as moviesApi from '../store/api/moviesApi';
import * as watchlistApi from '../store/api/watchlistApi';

jest.mock('../store/api/moviesApi', () => ({
  ...jest.requireActual('../store/api/moviesApi'),
  useGetPopularMoviesQuery: jest.fn(),
  useSearchMoviesQuery: jest.fn(),
}));

jest.mock('../store/api/watchlistApi', () => ({
  ...jest.requireActual('../store/api/watchlistApi'),
  useAddToWatchlistMutation: jest.fn(),
}));

jest.mock('../hooks/useWatchlistPresence', () => ({
  useWatchlistPresence: jest.fn(() => ({
    isInWatchlist: false,
    isLoading: false,
  })),
}));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useSearchParams: jest.fn(),
}));

const mockUseGetPopularMoviesQuery = moviesApi.useGetPopularMoviesQuery as jest.Mock;
const mockUseSearchMoviesQuery = moviesApi.useSearchMoviesQuery as jest.Mock;
const mockUseAddToWatchlistMutation = watchlistApi.useAddToWatchlistMutation as jest.Mock;
const mockedUseSearchParams = useSearchParams as jest.MockedFunction<typeof useSearchParams>;

describe('MoviesPage', () => {
  const mockAddMutation = jest.fn(() => Promise.resolve({ data: {} }));

  beforeEach(() => {
    jest.clearAllMocks();
    
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: {
        movies: mockMovies,
        totalResults: 3,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      isError: false,
      error: undefined,
      refetch: jest.fn(),
    });

    mockUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: undefined,
    });

    mockUseAddToWatchlistMutation.mockReturnValue([
      mockAddMutation,
      { isLoading: false },
    ]);

    mockedUseSearchParams.mockReturnValue([new URLSearchParams(), jest.fn()]);
  });

  it('should load popular movies on mount', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseGetPopularMoviesQuery).toHaveBeenCalledWith({ page: 1 });
    });
  });

  it('should render featured carousel with popular movies', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
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
    
    mockUseSearchMoviesQuery.mockReturnValue({
      data: {
        movies: [mockMovies[0]],
        totalResults: 1,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      isError: false,
      error: undefined,
    });

    mockedUseSearchParams.mockReturnValue([new URLSearchParams({ search: searchQuery }), jest.fn()]);

    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseSearchMoviesQuery).toHaveBeenCalledWith(
        { query: searchQuery, page: 1 },
        { skip: false }
      );
    });
  });

  it('should display error message on API failure', async () => {
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: { status: 500, data: { message: 'Failed to fetch movies' } },
      refetch: jest.fn(),
    });

    render(<MoviesPage />);

    await waitFor(() => {
      const errorAlert = screen.queryByRole('alert');
      expect(errorAlert).toBeInTheDocument();
    });
  });

  it('should auto-refresh popular movies every 3 minutes', async () => {
    jest.useFakeTimers();
    const mockRefetch = jest.fn();

    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: {
        movies: mockMovies,
        totalResults: 3,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      isError: false,
      error: undefined,
      refetch: mockRefetch,
    });

    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseGetPopularMoviesQuery).toHaveBeenCalled();
    });

    act(() => {
      jest.advanceTimersByTime(3 * 60 * 1000);
    });

    await waitFor(() => {
      expect(mockRefetch).toHaveBeenCalled();
    });

    jest.useRealTimers();
  });
});
