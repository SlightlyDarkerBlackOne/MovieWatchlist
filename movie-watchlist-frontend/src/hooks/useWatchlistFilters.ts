import { useMemo } from 'react';
import { WatchlistItem, WatchlistStatus } from '../types/watchlist.types';

export interface UseWatchlistFiltersProps {
  watchlist: WatchlistItem[];
  activeTab: number;
  statusFilter: number | 'all';
}

export interface UseWatchlistFiltersReturn {
  filteredItems: WatchlistItem[];
  allCount: number;
  favoritesCount: number;
  watchedCount: number;
}

export function useWatchlistFilters({
  watchlist,
  activeTab,
  statusFilter,
}: UseWatchlistFiltersProps): UseWatchlistFiltersReturn {
  const filteredItems = useMemo(() => {
    let filtered = [...watchlist];

    if (activeTab === 1) {
      filtered = filtered.filter(item => item.isFavorite);
    } else if (activeTab === 2) {
      filtered = filtered.filter(item => item.status === WatchlistStatus.Watched);
    }

    if (statusFilter !== 'all') {
      filtered = filtered.filter(item => item.status === statusFilter);
    }

    return filtered;
  }, [watchlist, activeTab, statusFilter]);

  const allCount = useMemo(() => watchlist.length, [watchlist]);
  
  const favoritesCount = useMemo(
    () => watchlist.filter(item => item.isFavorite).length,
    [watchlist]
  );
  
  const watchedCount = useMemo(
    () => watchlist.filter(item => item.status === WatchlistStatus.Watched).length,
    [watchlist]
  );

  return {
    filteredItems,
    allCount,
    favoritesCount,
    watchedCount,
  };
}

