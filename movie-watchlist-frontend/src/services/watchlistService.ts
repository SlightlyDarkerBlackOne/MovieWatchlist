import api from './api';
import { API_ENDPOINTS } from '../utils/constants';
import { 
  WatchlistItem, 
  WatchlistStatistics, 
  AddToWatchlistRequest, 
  UpdateWatchlistRequest,
  WatchlistStatus
} from '../types/watchlist.types';
import cacheService from '../utils/cacheService';
import { AxiosError } from '../types/error.types';

/**
 * Watchlist Status Configuration
 */
const STATUS_CONFIG = {
  [WatchlistStatus.Planned]: {
    label: 'Planned',
    color: 'info' as const,
  },
  [WatchlistStatus.Watching]: {
    label: 'Watching',
    color: 'primary' as const,
  },
  [WatchlistStatus.Watched]: {
    label: 'Watched',
    color: 'success' as const,
  },
  [WatchlistStatus.Dropped]: {
    label: 'Dropped',
    color: 'error' as const,
  },
} as const;

const DEFAULT_STATUS = {
  label: 'Unknown',
  color: 'default' as const,
};

// Cache configuration
const WATCHLIST_CACHE_KEY = 'watchlist';
const WATCHLIST_CACHE_TTL_HOURS = 0.5; // 30 minutes

class WatchlistService {
  /**
   * Get user's watchlist (with caching)
   */
  async getUserWatchlist(userId: number, forceRefresh: boolean = false): Promise<WatchlistItem[]> {
    try {
      // Create user-specific cache key
      const cacheKey = `${WATCHLIST_CACHE_KEY}_${userId}`;
      
      // Clear cache if force refresh requested
      if (forceRefresh) {
        cacheService.clear(cacheKey);
      }
      
      // Check cache first
      const cached = cacheService.get<WatchlistItem[]>(cacheKey);
      if (cached) {
        console.log(`Watchlist for user ${userId} loaded from cache`);
        return cached;
      }
      
      // Cache miss - fetch from API
      console.log(`Fetching watchlist for user ${userId} from API`);
      const response = await api.get(API_ENDPOINTS.WATCHLIST.USER(userId));
      const watchlist = response.data;
      
      // Store in cache
      cacheService.set(cacheKey, watchlist, WATCHLIST_CACHE_TTL_HOURS);
      
      return watchlist;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get watchlist error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to get watchlist');
    }
  }
  
  /**
   * Clear watchlist cache for a user
   */
  clearWatchlistCache(userId: number): void {
    const cacheKey = `${WATCHLIST_CACHE_KEY}_${userId}`;
    cacheService.clear(cacheKey);
    console.log(`Cleared watchlist cache for user ${userId}`);
  }

  /**
   * Update watchlist cache by removing a specific item
   */
  removeItemFromCache(userId: number, itemId: number): void {
    const cacheKey = `${WATCHLIST_CACHE_KEY}_${userId}`;
    const cached = cacheService.get<WatchlistItem[]>(cacheKey);
    
    if (cached) {
      const updated = cached.filter(item => item.id !== itemId);
      cacheService.set(cacheKey, updated, WATCHLIST_CACHE_TTL_HOURS);
      console.log(`Removed item ${itemId} from watchlist cache`);
    }
  }

  /**
   * Update watchlist cache by adding a new item
   */
  addItemToCache(userId: number, newItem: WatchlistItem): void {
    const cacheKey = `${WATCHLIST_CACHE_KEY}_${userId}`;
    const cached = cacheService.get<WatchlistItem[]>(cacheKey);
    
    if (cached) {
      const updated = [...cached, newItem];
      cacheService.set(cacheKey, updated, WATCHLIST_CACHE_TTL_HOURS);
      console.log(`Added item to watchlist cache`);
    }
  }

  /**
   * Update watchlist cache by modifying an existing item
   */
  updateItemInCache(userId: number, itemId: number, updatedItem: WatchlistItem): void {
    const cacheKey = `${WATCHLIST_CACHE_KEY}_${userId}`;
    const cached = cacheService.get<WatchlistItem[]>(cacheKey);
    
    if (cached) {
      const updated = cached.map(item => item.id === itemId ? updatedItem : item);
      cacheService.set(cacheKey, updated, WATCHLIST_CACHE_TTL_HOURS);
      console.log(`Updated item ${itemId} in watchlist cache`);
    }
  }

  /**
   * Add movie to watchlist
   */
  async addToWatchlist(userId: number, request: AddToWatchlistRequest): Promise<WatchlistItem> {
    try {
      const response = await api.post(API_ENDPOINTS.WATCHLIST.ADD(userId), request);
      const newItem = response.data;
      
      // Optimistically update cache with new item
      this.addItemToCache(userId, newItem);
      
      return newItem;
    } catch (error) {
      const axiosError = error as AxiosError;
      
      // Extract detailed error message
      let errorMessage = 'Failed to add to watchlist';
      if (axiosError.response?.data?.errors) {
        // ModelState validation errors
        const errors = Object.values(axiosError.response.data.errors).flat();
        errorMessage = errors.join(', ');
      } else if (axiosError.response?.data?.message) {
        errorMessage = axiosError.response.data.message;
      } else if (axiosError.response?.data) {
        errorMessage = JSON.stringify(axiosError.response.data);
      }
      
      throw new Error(errorMessage);
    }
  }

  /**
   * Update watchlist item
   */
  async updateWatchlistItem(
    userId: number, 
    itemId: number, 
    request: UpdateWatchlistRequest
  ): Promise<WatchlistItem> {
    try {
      const response = await api.put(API_ENDPOINTS.WATCHLIST.ITEM(userId, itemId), request);
      const updatedItem = response.data;
      
      // Optimistically update cache with modified item
      this.updateItemInCache(userId, itemId, updatedItem);
      
      return updatedItem;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Update watchlist error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to update watchlist item');
    }
  }

  /**
   * Remove item from watchlist
   */
  async removeFromWatchlist(userId: number, itemId: number): Promise<void> {
    try {
      await api.delete(API_ENDPOINTS.WATCHLIST.ITEM(userId, itemId));
      
      // Optimistically update cache by removing the item
      this.removeItemFromCache(userId, itemId);
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Remove from watchlist error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to remove from watchlist');
    }
  }

  /**
   * Get watchlist statistics
   */
  async getWatchlistStatistics(userId: number): Promise<WatchlistStatistics> {
    try {
      const response = await api.get(API_ENDPOINTS.WATCHLIST.STATISTICS(userId));
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get statistics error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to get statistics');
    }
  }

  /**
   * Get favorite movies
   */
  async getFavorites(userId: number): Promise<WatchlistItem[]> {
    try {
      const response = await api.get(API_ENDPOINTS.WATCHLIST.FAVORITES(userId));
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get favorites error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to get favorites');
    }
  }

  /**
   * Get watchlist items by genre
   */
  async getByGenre(userId: number, genre: string): Promise<WatchlistItem[]> {
    try {
      const response = await api.get(API_ENDPOINTS.WATCHLIST.BY_GENRE(userId, genre));
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get by genre error:', error);
      throw new Error(axiosError.response?.data?.message || 'Failed to get watchlist by genre');
    }
  }

  /**
   * Get status label
   * @param status - The watchlist status enum value
   * @returns Human-readable status label
   */
  getStatusLabel(status: WatchlistStatus): string {
    const config = STATUS_CONFIG[status];
    return config?.label || DEFAULT_STATUS.label;
  }

  /**
   * Get status color for chips/badges
   * @param status - The watchlist status enum value
   * @returns MUI color for the status
   */
  getStatusColor(status: WatchlistStatus): 'default' | 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success' {
    const config = STATUS_CONFIG[status];
    return config?.color || DEFAULT_STATUS.color;
  }

  /**
   * Get all available statuses with their labels and colors
   * @returns Array of status configurations
   */
  getAllStatuses() {
    return Object.entries(STATUS_CONFIG).map(([value, config]) => ({
      value: Number(value) as WatchlistStatus,
      label: config.label,
      color: config.color,
    }));
  }
}

export default new WatchlistService();

