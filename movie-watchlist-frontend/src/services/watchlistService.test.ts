/**
 * Tests for watchlist service
 */

import * as watchlistService from './watchlistService';
import api from './api';
import { mockWatchlistItems, mockWatchlistItem } from '../__tests__/fixtures/watchlistFixtures';
import { WatchlistStatus } from '../types/watchlist.types';

// Mock dependencies
jest.mock('./api');

const mockedApi = api as jest.Mocked<typeof api>;

describe('WatchlistService', () => {
  const userId = 1;

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getUserWatchlist', () => {
    it('should fetch watchlist from API', async () => {
      mockedApi.get.mockResolvedValue({ data: mockWatchlistItems });

      const result = await watchlistService.getUserWatchlist(userId);

      expect(result).toEqual(mockWatchlistItems);
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
    it('should add movie to watchlist', async () => {
      const request = {
        movieId: 550,
        status: WatchlistStatus.Planned,
        notes: 'Must watch!',
      };

      mockedApi.post.mockResolvedValue({ data: mockWatchlistItem });

      const result = await watchlistService.addToWatchlist(userId, request);

      expect(result).toEqual(mockWatchlistItem);
      expect(mockedApi.post).toHaveBeenCalled();
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
    it('should update watchlist item', async () => {
      const updatedItem = { ...mockWatchlistItem, userRating: 4.5 };
      mockedApi.put.mockResolvedValue({ data: updatedItem });

      const result = await watchlistService.updateWatchlistItem(userId, 1, {
        userRating: 4.5,
      });

      expect(result).toEqual(updatedItem);
      expect(mockedApi.put).toHaveBeenCalled();
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
    it('should remove item', async () => {
      mockedApi.delete.mockResolvedValue({ data: null });

      await watchlistService.removeFromWatchlist(userId, 1);

      expect(mockedApi.delete).toHaveBeenCalled();
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

  describe('getWatchlistStatistics', () => {
    it('should fetch statistics from API', async () => {
      const mockStats = {
        totalMovies: 10,
        watchedMovies: 5,
        plannedMovies: 3,
        favoriteMovies: 3,
        averageUserRating: 4.5,
        averageTmdbRating: 7.2,
        mostWatchedGenre: 'Action',
        moviesThisYear: 2,
        genreBreakdown: { Action: 3, Comedy: 2 },
        yearlyBreakdown: { 2024: 2 }
      };

      mockedApi.get.mockResolvedValue({ data: mockStats });

      const result = await watchlistService.getWatchlistStatistics(userId);

      expect(result).toEqual(mockStats);
      expect(mockedApi.get).toHaveBeenCalled();
    });
  });

  describe('getFavorites', () => {
    it('should fetch favorites from API', async () => {
      mockedApi.get.mockResolvedValue({ data: mockWatchlistItems });

      const result = await watchlistService.getFavorites(userId);

      expect(result).toEqual(mockWatchlistItems);
      expect(mockedApi.get).toHaveBeenCalled();
    });
  });

  describe('getByGenre', () => {
    it('should fetch watchlist by genre from API', async () => {
      mockedApi.get.mockResolvedValue({ data: mockWatchlistItems });

      const result = await watchlistService.getByGenre(userId, 'Action');

      expect(result).toEqual(mockWatchlistItems);
      expect(mockedApi.get).toHaveBeenCalled();
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
