import React from 'react';
import { renderHook, waitFor } from '@testing-library/react';
import { useFeaturedMovies } from '../useFeaturedMovies';
import { useGetPopularMoviesQuery } from '../../store/api/moviesApi';
import { AllProviders } from '../../utils/test-utils';
import { mockMovies } from '../../__tests__/fixtures/movieFixtures';
import { TestConstants } from '../../__tests__/TestConstants';

jest.mock('../../store/api/moviesApi', () => ({
  ...jest.requireActual('../../store/api/moviesApi'),
  useGetPopularMoviesQuery: jest.fn(),
}));

const mockUseGetPopularMoviesQuery = useGetPopularMoviesQuery as jest.MockedFunction<typeof useGetPopularMoviesQuery>;

describe('useFeaturedMovies', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should initialize with empty featured movies', () => {
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useFeaturedMovies(), { wrapper });

    expect(result.current.featuredMovies).toEqual([]);
    expect(result.current.featuredMovieIds).toEqual([]);
  });

  it('should extract featured movies from popular movies', async () => {
    const popularMoviesData = { movies: mockMovies, totalResults: mockMovies.length };
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: popularMoviesData,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useFeaturedMovies(), { wrapper });

    await waitFor(() => {
      const expectedLength = Math.min(TestConstants.TestValues.FeaturedMoviesCount, mockMovies.length);
      expect(result.current.featuredMovies).toHaveLength(expectedLength);
      expect(result.current.featuredMovies).toEqual(mockMovies.slice(0, expectedLength));
    });
  });

  it('should compute featured movie IDs', () => {
    const popularMoviesData = { movies: mockMovies.slice(0, 3), totalResults: 3 };
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: popularMoviesData,
      isLoading: false,
      error: undefined,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useFeaturedMovies(), { wrapper });

    expect(result.current.featuredMovieIds).toEqual(
      mockMovies.slice(0, 3).map(m => m.tmdbId)
    );
  });

  it('should expose popular movies error', () => {
    const mockError = { message: TestConstants.ErrorMessages.FailedToLoad };
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: mockError,
      refetch: jest.fn(),
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useFeaturedMovies(), { wrapper });

    expect(result.current.popularMoviesError).toEqual(mockError);
  });

  it('should expose refetch function', () => {
    const mockRefetch = jest.fn();
    mockUseGetPopularMoviesQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
      refetch: mockRefetch,
    } as any);

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AllProviders>{children}</AllProviders>
    );
    const { result } = renderHook(() => useFeaturedMovies(), { wrapper });

    expect(result.current.refetchPopularMovies).toBe(mockRefetch);
  });
});

