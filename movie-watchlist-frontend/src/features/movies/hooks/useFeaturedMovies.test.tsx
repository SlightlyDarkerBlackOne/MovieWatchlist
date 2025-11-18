/**
 * Tests for useFeaturedMovies hook
 */

import { renderHook, waitFor } from '@testing-library/react';
import { useFeaturedMovies } from './useFeaturedMovies';
import { useGetPopularMoviesQuery } from '../api/moviesApi';
import { mockMovies } from '../../../__tests__/fixtures/movieFixtures';

jest.mock('../api/moviesApi', () => ({
  useGetPopularMoviesQuery: jest.fn(),
}));

const mockedUseGetPopularMoviesQuery = useGetPopularMoviesQuery as jest.MockedFunction<typeof useGetPopularMoviesQuery>;

describe('useFeaturedMovies', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should initialize with empty featured movies', () => {
    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.featuredMovies).toEqual([]);
    expect(result.current.featuredMovieIds).toEqual([]);
    expect(result.current.popularMoviesLoading).toBe(false);
  });

  it('should set featured movies from popular movies data', async () => {
    const mockPopularMovies = {
      movies: mockMovies,
      totalResults: mockMovies.length,
      totalPages: 1,
      currentPage: 1,
    };

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: mockPopularMovies,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    await waitFor(() => {
      expect(result.current.featuredMovies).toHaveLength(3);
      expect(result.current.featuredMovies).toEqual(mockMovies.slice(0, 3));
    });

    expect(result.current.featuredMovieIds).toEqual(mockMovies.slice(0, 3).map(m => m.tmdbId));
  });

  it('should handle empty popular movies array', () => {
    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: {
        movies: [],
        totalResults: 0,
        totalPages: 0,
        currentPage: 1,
      },
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.featuredMovies).toEqual([]);
    expect(result.current.featuredMovieIds).toEqual([]);
  });

  it('should handle less than 5 movies', () => {
    const fewMovies = mockMovies.slice(0, 3);
    const mockPopularMovies = {
      movies: fewMovies,
      totalResults: fewMovies.length,
      totalPages: 1,
      currentPage: 1,
    };

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: mockPopularMovies,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.featuredMovies).toHaveLength(3);
    expect(result.current.featuredMovieIds).toEqual(fewMovies.map(m => m.tmdbId));
  });

  it('should expose loading state', () => {
    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.popularMoviesLoading).toBe(true);
  });

  it('should expose error state', () => {
    const mockError = { status: 500, data: { message: 'Server error' } };

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.popularMoviesError).toEqual(mockError);
  });

  it('should expose refetch function', () => {
    const mockRefetch = jest.fn();

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: mockRefetch,
    } as any);

    const { result } = renderHook(() => useFeaturedMovies());

    expect(result.current.refetchPopularMovies).toBe(mockRefetch);
  });

  it('should update featured movies when popular movies data changes', async () => {
    const initialMovies = mockMovies.slice(0, 2);
    const additionalMovies = [
      ...mockMovies,
      {
        ...mockMovies[0],
        id: 4,
        tmdbId: 100,
        title: 'Movie 4',
      },
      {
        ...mockMovies[0],
        id: 5,
        tmdbId: 101,
        title: 'Movie 5',
      },
    ];

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: {
        movies: initialMovies,
        totalResults: initialMovies.length,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const { result, rerender } = renderHook(() => useFeaturedMovies());

    await waitFor(() => {
      expect(result.current.featuredMovies).toHaveLength(2);
    });

    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: {
        movies: additionalMovies,
        totalResults: additionalMovies.length,
        totalPages: 1,
        currentPage: 1,
      },
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    rerender();

    await waitFor(() => {
      expect(result.current.featuredMovies).toHaveLength(5);
    });
  });

  it('should configure polling interval', () => {
    mockedUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    renderHook(() => useFeaturedMovies());

    expect(mockedUseGetPopularMoviesQuery).toHaveBeenCalledWith(
      { page: 1 },
      expect.objectContaining({
        pollingInterval: expect.any(Number),
      })
    );
  });
});

