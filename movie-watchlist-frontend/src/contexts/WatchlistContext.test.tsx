/**
 * Tests for WatchlistContext
 * 
 * NOTE: Updated for Set-based implementation (O(1) lookups) instead of array
 */

import React from 'react';
import { renderHook, waitFor } from '@testing-library/react';
import { WatchlistProvider, useWatchlist } from './WatchlistContext';
import { AuthProvider } from './AuthContext';
import { mockWatchlistItems } from '../__tests__/fixtures/watchlistFixtures';
import { mockUser } from '../__tests__/fixtures/authFixtures';
import * as watchlistApi from '../store/api/watchlistApi';

jest.mock('../store/api/watchlistApi', () => ({
  ...jest.requireActual('../store/api/watchlistApi'),
  useGetWatchlistQuery: jest.fn(),
}));

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

  it('should provide watchlistMovieIds as Set from RTK Query', async () => {
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toBeDefined();
    });

    const expectedIds = mockWatchlistItems
      .map(item => item.movie?.tmdbId)
      .filter((id): id is number => id !== undefined);
    
    expect(result.current.watchlistMovieIds).toBeInstanceOf(Set);
    expect(result.current.watchlistMovieIds.size).toBe(expectedIds.length);
    expectedIds.forEach(id => {
      expect(result.current.watchlistMovieIds.has(id)).toBe(true);
    });
  });

  it('should return empty Set when no watchlist data', async () => {
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toBeDefined();
    });

    expect(result.current.watchlistMovieIds).toBeInstanceOf(Set);
    expect(result.current.watchlistMovieIds.size).toBe(0);
  });

  it('should check if movie is in watchlist using O(1) Set lookup', async () => {
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.size).toBeGreaterThan(0);
    });

    const movieId = mockWatchlistItems[0].movie!.tmdbId;
    const isInWatchlist = result.current.isInWatchlist(movieId);

    expect(isInWatchlist).toBe(true);
  });

  it('should return false for movie not in watchlist', async () => {
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.size).toBeGreaterThan(0);
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

    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: itemsWithUndefined,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toBeDefined();
    });

    const movieId = mockWatchlistItems[1].movie!.tmdbId;
    expect(result.current.watchlistMovieIds.has(movieId)).toBe(true);
    expect(result.current.watchlistMovieIds.has(undefined as any)).toBe(false);
  });

  it('should provide O(1) lookup performance with Set', async () => {
    (watchlistApi.useGetWatchlistQuery as jest.Mock).mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isFetching: false,
      error: undefined,
      status: 'fulfilled' as const,
      isSuccess: true,
      isError: false,
      isUninitialized: false,
      refetch: jest.fn(),
    });

    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.size).toBeGreaterThan(0);
    });

    const movieId = mockWatchlistItems[0].movie!.tmdbId;
    
    const start = performance.now();
    result.current.isInWatchlist(movieId);
    const end = performance.now();
    
    expect(end - start).toBeLessThan(1);
  });
});
