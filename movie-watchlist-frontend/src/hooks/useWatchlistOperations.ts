import { useCallback } from 'react';
import {
  useGetWatchlistQuery,
  useAddToWatchlistMutation,
  useUpdateWatchlistItemMutation,
  useRemoveFromWatchlistMutation,
  useGetWatchlistStatisticsQuery
} from '../store/api/watchlistApi';
import { AddToWatchlistRequest, UpdateWatchlistRequest } from '../types/watchlist.types';

export function useWatchlistQuery(userId: number | undefined) {
  return useGetWatchlistQuery(userId!, { skip: !userId });
}

export function useWatchlistStatistics(userId: number | undefined) {
  return useGetWatchlistStatisticsQuery(userId!, { skip: !userId });
}

export function useAddToWatchlistOperation() {
  const [addMutation, { isLoading, error }] = useAddToWatchlistMutation();

  const addToWatchlist = useCallback(async (
    userId: number,
    request: AddToWatchlistRequest
  ) => {
    try {
      return await addMutation({ userId, request }).unwrap();
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to add to watchlist');
    }
  }, [addMutation]);

  return { addToWatchlist, isLoading, error };
}

export function useUpdateWatchlistItemOperation() {
  const [updateMutation, { isLoading, error }] = useUpdateWatchlistItemMutation();

  const updateItem = useCallback(async (
    userId: number,
    itemId: number,
    request: UpdateWatchlistRequest
  ) => {
    try {
      return await updateMutation({ userId, itemId, request }).unwrap();
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update watchlist item');
    }
  }, [updateMutation]);

  return { updateItem, isLoading, error };
}

export function useRemoveFromWatchlistOperation() {
  const [removeMutation, { isLoading, error }] = useRemoveFromWatchlistMutation();

  const removeItem = useCallback(async (userId: number, itemId: number) => {
    try {
      await removeMutation({ userId, itemId }).unwrap();
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to remove from watchlist');
    }
  }, [removeMutation]);

  return { removeItem, isLoading, error };
}


