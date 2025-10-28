/**
 * Tests for WatchlistContext
 */

import React from 'react';
import { renderHook, waitFor } from '@testing-library/react';
import { WatchlistProvider, useWatchlist } from './WatchlistContext';
import { AuthProvider } from './AuthContext';
import { mockWatchlistItems } from '../__tests__/fixtures/watchlistFixtures';
import { mockUser } from '../__tests__/fixtures/authFixtures';
import * as useWatchlistOperations from '../hooks/useWatchlistOperations';

jest.mock('../hooks/useWatchlistOperations');

const mockedUseWatchlistOperations = useWatchlistOperations as jest.Mocked<typeof useWatchlistOperations>;

// Mock AuthContext
jest.mock('./AuthContext', () => ({
  ...jest.requireActual('./AuthContext'),
  useAuth: () => ({
    user: mockUser,
    login: jest.fn(),
    logout: jest.fn(),
    register: jest.fn(),
    isAuthenticated: jest.fn(() => true),
    validateToken: jest.fn(),
  }),
}));

describe('WatchlistContext', () => {
  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <AuthProvider>
      <WatchlistProvider>{children}</WatchlistProvider>
    </AuthProvider>
  );

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should provide watchlistMovieIds from RTK Query', async () => {
    mockedUseWatchlistOperations.useWatchlistQuery.mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toBeDefined();
    });

    const expectedIds = mockWatchlistItems
      .map(item => item.movie?.tmdbId)
      .filter((id): id is number => id !== undefined);
    
    expect(result.current.watchlistMovieIds).toEqual(expectedIds);
  });

  it('should return empty array when no watchlist data', async () => {
    mockedUseWatchlistOperations.useWatchlistQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toEqual([]);
    });
  });

  it('should check if movie is in watchlist', async () => {
    mockedUseWatchlistOperations.useWatchlistQuery.mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.length).toBeGreaterThan(0);
    });

    const movieId = mockWatchlistItems[0].movie!.tmdbId;
    const isInWatchlist = result.current.isInWatchlist(movieId);

    expect(isInWatchlist).toBe(true);
  });

  it('should return false for movie not in watchlist', async () => {
    mockedUseWatchlistOperations.useWatchlistQuery.mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.length).toBeGreaterThan(0);
    });

    const nonExistentMovieId = 999999;
    const isInWatchlist = result.current.isInWatchlist(nonExistentMovieId);

    expect(isInWatchlist).toBe(false);
  });

  it('should handle undefined tmdbId values in watchlist', async () => {
    const itemsWithUndefined: any = [
      { ...mockWatchlistItems[0], movie: { ...mockWatchlistItems[0].movie!, tmdbId: undefined } },
      ...mockWatchlistItems.slice(1),
    ];

    mockedUseWatchlistOperations.useWatchlistQuery.mockReturnValue({
      data: itemsWithUndefined,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    } as any);

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toBeDefined();
    });

    const movieId = mockWatchlistItems[1].movie!.tmdbId;
    expect(result.current.watchlistMovieIds).toContain(movieId);
    expect(result.current.watchlistMovieIds).not.toContain(undefined);
  });
});
