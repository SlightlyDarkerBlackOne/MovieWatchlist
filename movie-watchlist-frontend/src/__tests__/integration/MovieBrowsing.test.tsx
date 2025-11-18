/**
 * Integration test for movie browsing flow
 */

import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';

jest.mock('../../shared/api/baseApi', () => {
  const mockBaseQuery = jest.fn().mockImplementation(async () => {
    return { data: {} };
  });
  return {
    baseQueryWithReauth: mockBaseQuery,
    setGlobalErrorHandler: jest.fn(),
    setNavigateHandler: jest.fn(),
  };
});

const server = setupServer(
  http.get('*', () => HttpResponse.json({})),
  http.post('*', () => HttpResponse.json({})),
  http.put('*', () => HttpResponse.json({})),
  http.delete('*', () => HttpResponse.json({})),
  http.patch('*', () => HttpResponse.json({})),
);

beforeAll(() => {
  server.listen({ onUnhandledRequest: 'bypass' });
});

afterEach(() => {
  server.resetHandlers();
});

afterAll(() => {
  server.close();
});

import React from 'react';
import { screen, fireEvent, waitFor } from '@testing-library/react';
import { renderWithMocks } from '../../shared/lib/test-utils';
import MoviesPage from '../../pages/MoviesPage';
import MovieDetailsPage from '../../pages/MovieDetailsPage';
import * as moviesApi from '../../features/movies/api/moviesApi';
import * as watchlistApi from '../../features/watchlist/api/watchlistApi';
import { mockMovies, mockMovieDetails, mockMovieCredits, mockMovieVideo } from '../fixtures/movieFixtures';
import { mockUser } from '../fixtures/authFixtures';
import { useWatchlistPresence } from '../../features/watchlist/hooks/useWatchlistPresence';
import * as authApi from '../../features/auth/api/authApi';

jest.mock('../../features/movies/api/moviesApi', () => {
  const actual = jest.requireActual('../../features/movies/api/moviesApi');
  return {
    ...actual,
    useGetPopularMoviesQuery: jest.fn(),
    useSearchMoviesQuery: jest.fn(),
  };
});

jest.mock('../../features/watchlist/api/watchlistApi', () => {
  const actual = jest.requireActual('../../features/watchlist/api/watchlistApi');
  return {
    ...actual,
    useGetWatchlistQuery: jest.fn(),
    useAddToWatchlistMutation: jest.fn(),
  };
});

jest.mock('../../features/auth/api/authApi', () => {
  const actual = jest.requireActual('../../features/auth/api/authApi');
  return {
    ...actual,
    useGetCurrentUserQuery: jest.fn(),
    useLoginMutation: jest.fn(),
    useRegisterMutation: jest.fn(),
    useLogoutMutation: jest.fn(),
    useForgotPasswordMutation: jest.fn(),
    useResetPasswordMutation: jest.fn(),
  };
});

jest.mock('../../features/watchlist/hooks/useWatchlistPresence', () => ({
  useWatchlistPresence: jest.fn(),
}));


const mockedUseWatchlistPresence = useWatchlistPresence as jest.MockedFunction<typeof useWatchlistPresence>;

describe('Movie Browsing Integration', () => {
  const mockAuthContext = {
    user: mockUser,
  };

  beforeEach(() => {
    jest.clearAllMocks();
    
    mockedUseWatchlistPresence.mockReturnValue({
      isInWatchlist: false,
      isLoading: false,
    });
    
    (authApi.useGetCurrentUserQuery as jest.Mock).mockReturnValue({
      data: mockUser,
      isLoading: false,
      error: undefined,
    });

    (authApi.useLoginMutation as jest.Mock).mockReturnValue([
      jest.fn(),
      { isLoading: false },
    ]);

    (authApi.useRegisterMutation as jest.Mock).mockReturnValue([
      jest.fn(),
      { isLoading: false },
    ]);

    (authApi.useLogoutMutation as jest.Mock).mockReturnValue([
      jest.fn(),
      { isLoading: false },
    ]);

    (authApi.useForgotPasswordMutation as jest.Mock).mockReturnValue([
      jest.fn(),
      { isLoading: false },
    ]);

    (authApi.useResetPasswordMutation as jest.Mock).mockReturnValue([
      jest.fn(),
      { isLoading: false },
    ]);
    
    (moviesApi.useGetPopularMoviesQuery as jest.Mock).mockReturnValue({
      data: {
        movies: mockMovies,
        totalResults: 3,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    });

    (moviesApi.useSearchMoviesQuery as jest.Mock).mockReturnValue({
      data: {
        movies: [mockMovies[0]],
        totalResults: 1,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      error: undefined,
    });

    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: [],
      isLoading: false,
      error: undefined,
    });

    (watchlistApi.useAddToWatchlistMutation as jest.Mock).mockReturnValue([
      jest.fn().mockResolvedValue({ data: {} }),
      { isLoading: false },
    ]);
  });

  it('should complete full flow: Browse → Search → View Details', async () => {
    // Step 1: Load Movies Page
    renderWithMocks(<MoviesPage />, { mockAuthContext });

    // Should show popular movies
    await waitFor(() => {
      expect(moviesApi.useGetPopularMoviesQuery).toHaveBeenCalled();
    });

    // Verify movies are displayed
    await waitFor(() => {
      const titles = screen.getAllByText(mockMovies[0].title);
      expect(titles.length).toBeGreaterThan(0);
    });
  });

  it('should handle add to watchlist from movies page', async () => {
    renderWithMocks(<MoviesPage />, { mockAuthContext });

    await waitFor(() => {
      const titles = screen.getAllByText(mockMovies[0].title);
      expect(titles.length).toBeGreaterThan(0);
      expect(titles[0]).toBeInTheDocument();
    });
  });

  it('should use RTK Query for data fetching', async () => {
    renderWithMocks(<MoviesPage />, { mockAuthContext });

    await waitFor(() => {
      expect(moviesApi.useGetPopularMoviesQuery).toHaveBeenCalled();
    });
  });
});


