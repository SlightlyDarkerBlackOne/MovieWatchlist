import { useMemo } from 'react';
import { useAuth } from '../../../features/auth/contexts/AuthContext';
import { useGetWatchlistQuery } from '../api/watchlistApi';

export const useWatchlistPresence = (tmdbId: number) => {
  const { user } = useAuth();
  
  const { data: watchlistItems, isLoading } = useGetWatchlistQuery(undefined, { 
    skip: !user 
  });
  
  const isInWatchlist = useMemo(() => {
    if (!watchlistItems || !tmdbId) return false;
    return watchlistItems.some(item => item.movie?.tmdbId === tmdbId);
  }, [watchlistItems, tmdbId]);
  
  return {
    isInWatchlist,
    isLoading,
  };
};


