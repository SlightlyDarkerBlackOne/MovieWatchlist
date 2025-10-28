import React, { createContext, useContext, useCallback, useMemo } from 'react';
import { useAuth } from './AuthContext';
import { useGetWatchlistQuery } from '../hooks/useWatchlistOperations';

export interface WatchlistContextType {
  watchlistMovieIds: Set<number>;
  isInWatchlist: (tmdbId: number) => boolean;
  addToWatchlist?: () => void;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

export const WatchlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const { data: watchlist } = useGetWatchlistQuery(user?.id ?? 0, { skip: !user });

  const watchlistMovieIds = useMemo(() => {
    if (!watchlist) return new Set<number>();
    const tmdbIds = watchlist
      .map(item => item.movie?.tmdbId)
      .filter((id): id is number => id !== undefined);
    return new Set(tmdbIds);
  }, [watchlist]);

  const isInWatchlist = useCallback((tmdbId: number) => {
    return watchlistMovieIds.has(tmdbId);
  }, [watchlistMovieIds]);

  const value: WatchlistContextType = useMemo(() => ({
    watchlistMovieIds,
    isInWatchlist,
  }), [watchlistMovieIds, isInWatchlist]);

  return (
    <WatchlistContext.Provider value={value}>
      {children}
    </WatchlistContext.Provider>
  );
};

export const useWatchlist = () => {
  const context = useContext(WatchlistContext);
  if (!context) {
    throw new Error('useWatchlist must be used within WatchlistProvider');
  }
  return context;
};

export default WatchlistContext;
