import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { useAuth } from '../contexts/AuthContext';
import { RootState } from '../store';
import { selectWatchlistMovieIds, selectWatchlistResult } from '../store/selectors/watchlistSelectors';

export const useWatchlistPresence = (tmdbId: number) => {
  const { user } = useAuth();
  
  const movieIds = useSelector((state: RootState) => {
    if (!user?.id) return new Set<number>();
    return selectWatchlistMovieIds(state, user.id);
  });
  
  const isLoading = useSelector((state: RootState) => {
    if (!user?.id) return false;
    const result = selectWatchlistResult(user.id)(state);
    return result?.isLoading ?? false;
  });
  
  const isInWatchlist = useMemo(() => {
    if (!user?.id || !tmdbId) return false;
    return movieIds.has(tmdbId);
  }, [user?.id, tmdbId, movieIds]);
  
  return {
    isInWatchlist,
    isLoading,
  };
};


