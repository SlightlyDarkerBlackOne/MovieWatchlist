/**
 * Tests for WatchlistContext
 */

import React from 'react';
import { renderHook, act, waitFor } from '@testing-library/react';
import { WatchlistProvider, useWatchlist } from './WatchlistContext';
import { AuthProvider } from './AuthContext';
import watchlistService from '../services/watchlistService';
import { mockMovie } from '../__tests__/fixtures/movieFixtures';
import { mockWatchlistItems } from '../__tests__/fixtures/watchlistFixtures';
import { mockUser } from '../__tests__/fixtures/authFixtures';
import { WatchlistStatus } from '../types/watchlist.types';

// Mock dependencies
jest.mock('../services/watchlistService');

const mockedWatchlistService = watchlistService as jest.Mocked<typeof watchlistService>;

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
    mockedWatchlistService.getUserWatchlist.mockResolvedValue(mockWatchlistItems);
  });

  it('should load watchlist IDs on mount', async () => {
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds).toHaveLength(mockWatchlistItems.length);
    });

    expect(mockedWatchlistService.getUserWatchlist).toHaveBeenCalledWith(mockUser.id);
  });

  it('should add movie to watchlist and update IDs', async () => {
    mockedWatchlistService.addToWatchlist.mockResolvedValue(mockWatchlistItems[0]);
    
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    expect(result.current.addDialogOpen).toBe(true);
    expect(result.current.selectedMovie).toEqual(mockMovie);
  });

  it('should show error when adding without login', async () => {
    // Temporarily override the useAuth mock for this test only
    const AuthContext = require('./AuthContext');
    const originalUseAuth = AuthContext.useAuth;
    
    // Mock useAuth to return null user for this test
    AuthContext.useAuth = jest.fn(() => ({
      user: null,
      login: jest.fn(),
      logout: jest.fn(),
      register: jest.fn(),
      isAuthenticated: jest.fn(() => false),
      validateToken: jest.fn(),
      forgotPassword: jest.fn(),
      resetPassword: jest.fn(),
      getToken: jest.fn(() => null),
    }));

    const wrapperWithoutAuth = ({ children }: { children: React.ReactNode }) => (
      <WatchlistProvider>{children}</WatchlistProvider>
    );

    const { result } = renderHook(() => useWatchlist(), { wrapper: wrapperWithoutAuth });

    act(() => {
      result.current.addToWatchlist(mockMovie);
    });

    await waitFor(() => {
      expect(result.current.error).toContain('log in');
    });

    // Restore original mock
    AuthContext.useAuth = originalUseAuth;
  });

  it('should handle confirm add to watchlist', async () => {
    mockedWatchlistService.addToWatchlist.mockResolvedValue(mockWatchlistItems[0]);
    
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    // Open dialog first
    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    // Set status and notes
    act(() => {
      result.current.setStatus(WatchlistStatus.Watching);
      result.current.setNotes('Test notes');
    });

    // Confirm add
    await act(async () => {
      await result.current.handleConfirmAdd();
    });

    await waitFor(() => {
      expect(mockedWatchlistService.addToWatchlist).toHaveBeenCalled();
      expect(result.current.successMessage).toContain('added to your watchlist');
      expect(result.current.addDialogOpen).toBe(false);
    });
  });

  it('should handle add errors', async () => {
    mockedWatchlistService.addToWatchlist.mockRejectedValue(
      new Error('Movie already in watchlist')
    );
    
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    await act(async () => {
      await result.current.handleConfirmAdd();
    });

    await waitFor(() => {
      expect(result.current.error).toContain('already in watchlist');
      expect(result.current.addDialogOpen).toBe(false);
    });
  });

  it('should close dialog and reset state', async () => {
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    act(() => {
      result.current.setNotes('Test notes');
      result.current.setStatus(WatchlistStatus.Watching);
    });

    act(() => {
      result.current.handleCloseDialog();
    });

    expect(result.current.addDialogOpen).toBe(false);
    expect(result.current.selectedMovie).toBeNull();
    expect(result.current.notes).toBe('');
    expect(result.current.status).toBe(WatchlistStatus.Planned);
  });

  it('should check if movie is in watchlist', async () => {
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.length).toBeGreaterThan(0);
    });

    const movieId = mockWatchlistItems[0].movie!.tmdbId;
    const isInWatchlist = result.current.isInWatchlist(movieId);

    expect(isInWatchlist).toBe(true);
  });

  it('should remove from watchlist IDs', async () => {
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await waitFor(() => {
      expect(result.current.watchlistMovieIds.length).toBeGreaterThan(0);
    });

    const movieId = mockWatchlistItems[0].movie!.tmdbId;

    act(() => {
      result.current.removeFromWatchlistIds(movieId);
    });

    expect(result.current.isInWatchlist(movieId)).toBe(false);
  });

  it('should refresh watchlist IDs', async () => {
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    await act(async () => {
      await result.current.refreshWatchlistIds();
    });

    expect(mockedWatchlistService.getUserWatchlist).toHaveBeenCalled();
  });

  it('should auto-hide success message after 3 seconds', async () => {
    jest.useFakeTimers();
    mockedWatchlistService.addToWatchlist.mockResolvedValue(mockWatchlistItems[0]);
    
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    // Open dialog and confirm add
    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    await act(async () => {
      await result.current.handleConfirmAdd();
    });

    // Wait for success message to appear
    await waitFor(() => {
      expect(result.current.successMessage).toBeTruthy();
    });

    // Advance timers and flush any pending timers
    await act(async () => {
      jest.advanceTimersByTime(3000);
    });

    // Success message should now be null
    expect(result.current.successMessage).toBeNull();

    jest.useRealTimers();
  });

  it('should auto-hide error message after 5 seconds', async () => {
    jest.useFakeTimers();
    mockedWatchlistService.addToWatchlist.mockRejectedValue(new Error('Test error'));
    
    const { result } = renderHook(() => useWatchlist(), { wrapper });

    // Open dialog and confirm add (which will fail)
    await act(async () => {
      result.current.addToWatchlist(mockMovie);
    });

    await act(async () => {
      await result.current.handleConfirmAdd();
    });

    // Wait for error message to appear
    await waitFor(() => {
      expect(result.current.error).toBeTruthy();
    });

    // Advance timers and flush any pending timers
    await act(async () => {
      jest.advanceTimersByTime(5000);
    });

    // Error message should now be null
    expect(result.current.error).toBeNull();

    jest.useRealTimers();
  });
});


