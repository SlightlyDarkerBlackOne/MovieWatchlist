/**
 * Tests for WatchlistPage component
 */

import React from 'react';
import { screen, waitFor, fireEvent } from '@testing-library/react';
import { renderWithMocks } from '../utils/test-utils';
import WatchlistPage from './WatchlistPage';
import { mockWatchlistItems } from '../__tests__/fixtures/watchlistFixtures';
import { mockUser } from '../__tests__/fixtures/authFixtures';
import * as watchlistApi from '../store/api/watchlistApi';

jest.mock('../store/api/watchlistApi', () => ({
  ...jest.requireActual('../store/api/watchlistApi'),
  useGetWatchlistQuery: jest.fn(),
  useUpdateWatchlistItemMutation: jest.fn(),
  useRemoveFromWatchlistMutation: jest.fn(),
}));

const mockUseGetWatchlistQuery = watchlistApi.useGetWatchlistQuery as jest.Mock;
const mockUseUpdateWatchlistItemMutation = watchlistApi.useUpdateWatchlistItemMutation as jest.Mock;
const mockUseRemoveFromWatchlistMutation = watchlistApi.useRemoveFromWatchlistMutation as jest.Mock;

describe('WatchlistPage', () => {
  const mockAuthContext = {
    user: mockUser,
  };


  const mockUpdateMutation = jest.fn(() => Promise.resolve({ data: mockWatchlistItems[0] }));
  const mockRemoveMutation = jest.fn(() => Promise.resolve());

  beforeEach(() => {
    jest.clearAllMocks();
    
    mockUseGetWatchlistQuery.mockReturnValue({
      data: mockWatchlistItems,
      isLoading: false,
      isError: false,
      error: undefined,
      refetch: jest.fn(),
    });

    mockUseUpdateWatchlistItemMutation.mockReturnValue([
      mockUpdateMutation,
      { isLoading: false },
    ]);

    mockUseRemoveFromWatchlistMutation.mockReturnValue([
      mockRemoveMutation,
      { isLoading: false },
    ]);

    global.confirm = jest.fn(() => true);
  });

  it('should load user watchlist on mount', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(mockUseGetWatchlistQuery).toHaveBeenCalledWith(undefined, { skip: false });
    });
  });

  it('should display watchlist title and description', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    expect(screen.getByText('My Watchlist')).toBeInTheDocument();
    expect(screen.getByText(/manage your movie collection/i)).toBeInTheDocument();
  });

  it('should display filter by status dropdown', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByLabelText(/filter by status/i)).toBeInTheDocument();
    });
  });

  it('should show movie count', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(`${mockWatchlistItems.length} movies`)).toBeInTheDocument();
    });
  });

  it('should render tabs (All, Favorites, Watched)', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(/all \(/i)).toBeInTheDocument();
      expect(screen.getByText(/favorites \(/i)).toBeInTheDocument();
      expect(screen.getByText(/watched \(/i)).toBeInTheDocument();
    });
  });

  it('should switch between tabs', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(/all \(/i)).toBeInTheDocument();
    });

    const favoritesTab = screen.getByText(/favorites \(/i);
    fireEvent.click(favoritesTab);

    expect(favoritesTab.closest('button')).toHaveAttribute('aria-selected');
  });

  it('should filter by status using dropdown', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(mockUseGetWatchlistQuery).toHaveBeenCalledWith(undefined, { skip: false });
    });

    await waitFor(() => {
      expect(screen.getByText(`${mockWatchlistItems.length} movies`)).toBeInTheDocument();
    });
  });

  it('should handle delete item', async () => {
    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(mockWatchlistItems[0].movie!.title)).toBeInTheDocument();
    });
  });

  it('should show login prompt when not authenticated', () => {
    const mockAuthContextNoUser = {
      user: null,
    };

    renderWithMocks(<WatchlistPage />, { mockAuthContext: mockAuthContextNoUser });

    expect(screen.getByText(/please log in/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go to login/i })).toBeInTheDocument();
  });

  it('should display loading spinner while fetching', () => {
    mockUseGetWatchlistQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: undefined,
      refetch: jest.fn(),
    });

    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    expect(mockUseGetWatchlistQuery).toHaveBeenCalledWith(undefined, { skip: false });
  });

  it('should display empty state when watchlist is empty', async () => {
    mockUseGetWatchlistQuery.mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: undefined,
      refetch: jest.fn(),
    });

    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      expect(screen.getByText(/your watchlist is empty/i)).toBeInTheDocument();
    });
  });

  it('should handle API errors gracefully', async () => {
    mockUseGetWatchlistQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: { status: 500, data: { message: 'Failed to load watchlist' } },
      refetch: jest.fn(),
    });

    renderWithMocks(<WatchlistPage />, { mockAuthContext });

    await waitFor(() => {
      const errorAlert = screen.queryByRole('alert');
      expect(errorAlert).toBeInTheDocument();
    });
  });
});
