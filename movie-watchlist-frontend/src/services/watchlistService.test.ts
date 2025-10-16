/**
 * Tests for watchlist service
 */

import watchlistService from './watchlistService';
import api from './api';
import cacheService from '../utils/cacheService';
import { mockWatchlistItems, mockWatchlistItem } from '../__tests__/fixtures/watchlistFixtures';
import { WatchlistStatus } from '../types/watchlist.types';

// Mock dependencies
jest.mock('./api');
jest.mock('../utils/cacheService');

const mockedApi = api as jest.Mocked<typeof api>;
const mockedCache = cacheService as jest.Mocked<typeof cacheService>;

describe('WatchlistService', () => {
  const userId = 1;

  beforeEach(() => {
    jest.clearAllMocks();
    mockedCache.get.mockReturnValue(null); // Default to cache miss
  });

  describe('getUserWatchlist', () => {
    it('should return cached watchlist when available', async () => {
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      const result = await watchlistService.getUserWatchlist(userId);

      expect(result).toEqual(mockWatchlistItems);
      expect(mockedApi.get).not.toHaveBeenCalled();
      expect(mockedCache.get).toHaveBeenCalledWith('watchlist_1');
    });

    it('should fetch from API on cache miss', async () => {
      mockedCache.get.mockReturnValue(null);
      mockedApi.get.mockResolvedValue({ data: mockWatchlistItems });

      const result = await watchlistService.getUserWatchlist(userId);

      expect(result).toEqual(mockWatchlistItems);
      expect(mockedApi.get).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalledWith(
        'watchlist_1',
        mockWatchlistItems,
        expect.any(Number)
      );
    });

    it('should force refresh when requested', async () => {
      // When forceRefresh=true, cache is cleared first, then get returns null (cache miss)
      mockedCache.get.mockReturnValue(null); // Cache miss after clear
      mockedApi.get.mockResolvedValue({ data: mockWatchlistItems });

      await watchlistService.getUserWatchlist(userId, true);

      expect(mockedCache.clear).toHaveBeenCalledWith('watchlist_1');
      expect(mockedApi.get).toHaveBeenCalled();
    });

    it('should throw error on API failure', async () => {
      mockedApi.get.mockRejectedValue({
        response: { data: { message: 'Server error' } }
      });

      await expect(watchlistService.getUserWatchlist(userId)).rejects.toThrow('Server error');
    });
  });

  describe('addToWatchlist', () => {
    it('should add movie to watchlist and update cache', async () => {
      const request = {
        movieId: 550,
        status: WatchlistStatus.Planned,
        notes: 'Must watch!',
      };

      mockedApi.post.mockResolvedValue({ data: mockWatchlistItem });
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      const result = await watchlistService.addToWatchlist(userId, request);

      expect(result).toEqual(mockWatchlistItem);
      expect(mockedApi.post).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalled(); // Optimistic cache update
    });

    it('should handle duplicate movie error', async () => {
      mockedApi.post.mockRejectedValue({
        response: { data: { message: 'Movie is already in watchlist' } }
      });

      await expect(
        watchlistService.addToWatchlist(userId, { movieId: 550 })
      ).rejects.toThrow('Movie is already in watchlist');
    });

    it('should handle validation errors', async () => {
      mockedApi.post.mockRejectedValue({
        response: {
          data: {
            errors: {
              movieId: ['Movie ID is required'],
              status: ['Invalid status value'],
            }
          }
        }
      });

      await expect(
        watchlistService.addToWatchlist(userId, { movieId: 0 })
      ).rejects.toThrow();
    });
  });

  describe('updateWatchlistItem', () => {
    it('should update watchlist item and cache', async () => {
      const updatedItem = { ...mockWatchlistItem, userRating: 4.5 };
      mockedApi.put.mockResolvedValue({ data: updatedItem });
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      const result = await watchlistService.updateWatchlistItem(userId, 1, {
        userRating: 4.5,
      });

      expect(result).toEqual(updatedItem);
      expect(mockedApi.put).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalled(); // Optimistic cache update
    });

    it('should handle update errors', async () => {
      mockedApi.put.mockRejectedValue({
        response: { data: { message: 'Item not found' } }
      });

      await expect(
        watchlistService.updateWatchlistItem(userId, 999, {})
      ).rejects.toThrow('Item not found');
    });
  });

  describe('removeFromWatchlist', () => {
    it('should remove item and update cache', async () => {
      mockedApi.delete.mockResolvedValue({ data: null });
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      await watchlistService.removeFromWatchlist(userId, 1);

      expect(mockedApi.delete).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalled(); // Optimistic cache update
    });

    it('should handle deletion errors', async () => {
      mockedApi.delete.mockRejectedValue({
        response: { data: { message: 'Item not found' } }
      });

      await expect(
        watchlistService.removeFromWatchlist(userId, 999)
      ).rejects.toThrow('Item not found');
    });
  });

  describe('cache management', () => {
    it('should clear watchlist cache for specific user', () => {
      watchlistService.clearWatchlistCache(userId);

      expect(mockedCache.clear).toHaveBeenCalledWith('watchlist_1');
    });

    it('should remove item from cache', () => {
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      watchlistService.removeItemFromCache(userId, 1);

      expect(mockedCache.get).toHaveBeenCalledWith('watchlist_1');
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should add item to cache', () => {
      mockedCache.get.mockReturnValue(mockWatchlistItems);

      watchlistService.addItemToCache(userId, mockWatchlistItem);

      expect(mockedCache.get).toHaveBeenCalledWith('watchlist_1');
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should update item in cache', () => {
      mockedCache.get.mockReturnValue(mockWatchlistItems);
      const updatedItem = { ...mockWatchlistItem, userRating: 5 };

      watchlistService.updateItemInCache(userId, 1, updatedItem);

      expect(mockedCache.get).toHaveBeenCalledWith('watchlist_1');
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should handle cache operations when cache is empty', () => {
      mockedCache.get.mockReturnValue(null);

      watchlistService.removeItemFromCache(userId, 1);
      watchlistService.addItemToCache(userId, mockWatchlistItem);
      watchlistService.updateItemInCache(userId, 1, mockWatchlistItem);

      // Should not throw errors
      expect(mockedCache.set).not.toHaveBeenCalled();
    });
  });

  describe('status helpers', () => {
    it('should return correct status label', () => {
      expect(watchlistService.getStatusLabel(WatchlistStatus.Planned)).toBe('Planned');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Watching)).toBe('Watching');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Watched)).toBe('Watched');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Dropped)).toBe('Dropped');
    });

    it('should return correct status color', () => {
      expect(watchlistService.getStatusColor(WatchlistStatus.Planned)).toBe('info');
      expect(watchlistService.getStatusColor(WatchlistStatus.Watching)).toBe('primary');
      expect(watchlistService.getStatusColor(WatchlistStatus.Watched)).toBe('success');
      expect(watchlistService.getStatusColor(WatchlistStatus.Dropped)).toBe('error');
    });

    it('should handle unknown status gracefully', () => {
      const unknownStatus = 999 as WatchlistStatus;
      expect(watchlistService.getStatusLabel(unknownStatus)).toBe('Unknown');
      expect(watchlistService.getStatusColor(unknownStatus)).toBe('default');
    });
  });
});


