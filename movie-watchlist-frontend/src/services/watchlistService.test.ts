/**
 * Tests for watchlist service utilities
 * 
 * NOTE: Network functions (getUserWatchlist, addToWatchlist, updateWatchlistItem, etc.)
 * have been moved to RTK Query in store/api/watchlistApi.ts
 * See __tests__/integration/watchlistApi.test.ts for integration tests
 */

import * as watchlistService from './watchlistService';
import { WatchlistStatus } from '../types/watchlist.types';

describe('WatchlistService Utilities', () => {

  describe('getStatusLabel', () => {
    it('should return correct status label', () => {
      expect(watchlistService.getStatusLabel(WatchlistStatus.Planned)).toBe('Planned');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Watching)).toBe('Watching');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Watched)).toBe('Watched');
      expect(watchlistService.getStatusLabel(WatchlistStatus.Dropped)).toBe('Dropped');
    });

    it('should handle unknown status gracefully', () => {
      const unknownStatus = 999 as WatchlistStatus;
      expect(watchlistService.getStatusLabel(unknownStatus)).toBe('Unknown');
    });
  });

  describe('getStatusColor', () => {
    it('should return correct status color', () => {
      expect(watchlistService.getStatusColor(WatchlistStatus.Planned)).toBe('info');
      expect(watchlistService.getStatusColor(WatchlistStatus.Watching)).toBe('primary');
      expect(watchlistService.getStatusColor(WatchlistStatus.Watched)).toBe('success');
      expect(watchlistService.getStatusColor(WatchlistStatus.Dropped)).toBe('error');
    });

    it('should handle unknown status gracefully', () => {
      const unknownStatus = 999 as WatchlistStatus;
      expect(watchlistService.getStatusColor(unknownStatus)).toBe('default');
    });
  });

  describe('getAllStatuses', () => {
    it('should return all statuses with labels and colors', () => {
      const statuses = watchlistService.getAllStatuses();
      
      expect(statuses).toHaveLength(4);
      expect(statuses).toEqual([
        { value: WatchlistStatus.Planned, label: 'Planned', color: 'info' },
        { value: WatchlistStatus.Watching, label: 'Watching', color: 'primary' },
        { value: WatchlistStatus.Watched, label: 'Watched', color: 'success' },
        { value: WatchlistStatus.Dropped, label: 'Dropped', color: 'error' },
      ]);
    });

    it('should return statuses with numeric values', () => {
      const statuses = watchlistService.getAllStatuses();
      
      statuses.forEach(status => {
        expect(typeof status.value).toBe('number');
        expect(typeof status.label).toBe('string');
        expect(typeof status.color).toBe('string');
      });
    });
  });
});
