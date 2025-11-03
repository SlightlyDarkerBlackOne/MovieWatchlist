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
import { useWatchlistPresence } from '../hooks/useWatchlistPresence';
import { TestConstants } from '../__tests__/TestConstants';

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
  useWatchlistPresence: jest.fn(),
}));

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useSearchParams: jest.fn(),
}));

const mockUseGetPopularMoviesQuery = moviesApi.useGetPopularMoviesQuery as jest.Mock;
const mockUseSearchMoviesQuery = moviesApi.useSearchMoviesQuery as jest.Mock;
const mockUseAddToWatchlistMutation = watchlistApi.useAddToWatchlistMutation as jest.Mock;
const mockedUseSearchParams = useSearchParams as jest.MockedFunction<typeof useSearchParams>;
const mockedUseWatchlistPresence = useWatchlistPresence as jest.MockedFunction<typeof useWatchlistPresence>;

describe('MoviesPage', () => {
  const mockAddMutation = jest.fn(() => Promise.resolve({ data: {} }));

  beforeEach(() => {
    jest.clearAllMocks();
    
    // Reset and set up the mocks
    mockUseGetPopularMoviesQuery.mockClear();
    mockUseSearchMoviesQuery.mockClear();
    mockUseAddToWatchlistMutation.mockClear();
    
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
      status: 'fulfilled',
      refetch: jest.fn(),
    });

    mockUseSearchMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: undefined,
      status: 'uninitialized',
    });

    mockUseAddToWatchlistMutation.mockReturnValue([
      mockAddMutation,
      { isLoading: false, isError: false, error: undefined, status: 'idle' },
    ]);

    mockedUseSearchParams.mockReturnValue([new URLSearchParams(), jest.fn()]);

    mockedUseWatchlistPresence.mockImplementation((tmdbId: number) => ({
      isInWatchlist: false,
      isLoading: false,
    }));
  });

  it('should load popular movies on mount', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseGetPopularMoviesQuery).toHaveBeenCalled();
    });
    
    expect(mockUseGetPopularMoviesQuery).toHaveBeenCalledWith(
      { page: TestConstants.TestValues.DefaultPage },
      expect.objectContaining({
        pollingInterval: expect.any(Number)
      })
    );
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
      expect(screen.getByText(TestConstants.UI.PopularMovies)).toBeInTheDocument();
    });
  });

  it('should show refresh button for popular movies', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      const refreshButton = screen.getByRole('button', { name: new RegExp(TestConstants.UI.RefreshPopularMovies, 'i') });
      expect(refreshButton).toBeInTheDocument();
    });
  });

  it('should handle search from URL params', async () => {
    const searchQuery = TestConstants.SearchQueries.FightClub;
    
    mockUseSearchMoviesQuery.mockReturnValue({
      data: {
        movies: [mockMovies[0]],
        totalResults: 1,
        totalPages: 1,
        currentPage: TestConstants.TestValues.DefaultPage,
      },
      isLoading: false,
      isError: false,
      error: undefined,
      status: 'fulfilled',
    });

    mockedUseSearchParams.mockReturnValue([new URLSearchParams({ search: searchQuery }), jest.fn()]);

    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseSearchMoviesQuery).toHaveBeenCalledWith(
        { query: searchQuery, page: TestConstants.TestValues.DefaultPage },
        { skip: false }
      );
    });
  });

  it('should display error message on API failure', async () => {
    const errorMessage = new Error(TestConstants.ErrorMessages.FailedToFetchMovies);
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: errorMessage,
      status: 'rejected',
      refetch: jest.fn(),
    });

    render(<MoviesPage />);

    await waitFor(() => {
      const errorText = screen.getByText(new RegExp(TestConstants.ErrorMessages.FailedToLoadPopularMovies, 'i'));
      expect(errorText).toBeInTheDocument();
    });
  });

  it('should configure polling interval for popular movies', async () => {
    render(<MoviesPage />);

    await waitFor(() => {
      expect(mockUseGetPopularMoviesQuery).toHaveBeenCalled();
    });

    expect(mockUseGetPopularMoviesQuery).toHaveBeenCalledWith(
      { page: TestConstants.TestValues.DefaultPage },
      expect.objectContaining({
        pollingInterval: TestConstants.TestValues.PollingIntervalMinutes * 60 * 1000
      })
    );
  });
});
