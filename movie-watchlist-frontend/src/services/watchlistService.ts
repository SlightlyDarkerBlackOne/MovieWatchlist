import api from './api';
import { API_ENDPOINTS } from '../utils/constants';
import { 
  WatchlistItem, 
  WatchlistStatistics, 
  AddToWatchlistRequest, 
  UpdateWatchlistRequest,
  WatchlistStatus
} from '../types/watchlist.types';
import { AxiosError } from '../types/error.types';

export const STATUS_CONFIG = {
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

export function getStatusLabel(status: WatchlistStatus): string {
  return STATUS_CONFIG[status]?.label || 'Unknown';
}

export function getStatusColor(status: WatchlistStatus): 'default' | 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success' {
  return STATUS_CONFIG[status]?.color || 'default';
}

export function getAllStatuses() {
  return Object.entries(STATUS_CONFIG).map(([value, config]) => ({
    value: Number(value) as WatchlistStatus,
    label: config.label,
    color: config.color,
  }));
}

export async function getUserWatchlist(userId: number): Promise<WatchlistItem[]> {
  try {
    const response = await api.get(API_ENDPOINTS.WATCHLIST.USER(userId));
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get watchlist error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to get watchlist');
  }
}

export async function addToWatchlist(userId: number, request: AddToWatchlistRequest): Promise<WatchlistItem> {
  try {
    const response = await api.post(API_ENDPOINTS.WATCHLIST.ADD(userId), request);
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    
    let errorMessage = 'Failed to add to watchlist';
    if (axiosError.response?.data?.errors) {
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

export async function updateWatchlistItem(
  userId: number, 
  itemId: number, 
  request: UpdateWatchlistRequest
): Promise<WatchlistItem> {
  try {
    const response = await api.put(API_ENDPOINTS.WATCHLIST.ITEM(userId, itemId), request);
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    
    let errorMessage = 'Failed to update watchlist item';
    if (axiosError.response?.data?.errors) {
      const errors = Array.isArray(axiosError.response.data.errors) 
        ? axiosError.response.data.errors 
        : Object.values(axiosError.response.data.errors).flat();
      errorMessage = errors.join(', ');
    } else if (axiosError.response?.data?.message) {
      errorMessage = axiosError.response.data.message;
    }
    
    console.error('Update watchlist error:', error);
    throw new Error(errorMessage);
  }
}

export async function removeFromWatchlist(userId: number, itemId: number): Promise<void> {
  try {
    await api.delete(API_ENDPOINTS.WATCHLIST.ITEM(userId, itemId));
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Remove from watchlist error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to remove from watchlist');
  }
}

export async function getWatchlistStatistics(userId: number): Promise<WatchlistStatistics> {
  try {
    const response = await api.get(API_ENDPOINTS.WATCHLIST.STATISTICS(userId));
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get statistics error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to get statistics');
  }
}

export async function getFavorites(userId: number): Promise<WatchlistItem[]> {
  try {
    const response = await api.get(API_ENDPOINTS.WATCHLIST.FAVORITES(userId));
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get favorites error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to get favorites');
  }
}

export async function getByGenre(userId: number, genre: string): Promise<WatchlistItem[]> {
  try {
    const response = await api.get(API_ENDPOINTS.WATCHLIST.BY_GENRE(userId, genre));
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get by genre error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to get watchlist by genre');
  }
}
