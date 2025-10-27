import React, { createContext, useContext, useCallback, useMemo } from 'react';
import { useAuth } from './AuthContext';
import { useWatchlistQuery } from '../hooks/useWatchlistOperations';

export interface WatchlistContextType {
  watchlistMovieIds: number[];
  isInWatchlist: (tmdbId: number) => boolean;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

export const WatchlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const { data: watchlist } = useWatchlistQuery(user?.id);

  const watchlistMovieIds = useMemo(() => {
    if (!watchlist) return [];
    return watchlist.map(item => item.movie?.tmdbId).filter((id): id is number => id !== undefined);
  }, [watchlist]);

  const isInWatchlist = useCallback((tmdbId: number) => {
    return watchlistMovieIds.includes(tmdbId);
  }, [watchlistMovieIds]);

  const value: WatchlistContextType = {
    watchlistMovieIds,
    isInWatchlist,
  };

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
