import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { Movie } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from './AuthContext';
import watchlistService from '../services/watchlistService';

export interface WatchlistContextType {
  // State
  watchlistMovieIds: number[];
  selectedMovie: Movie | null;
  addDialogOpen: boolean;
  status: WatchlistStatus;
  notes: string;
  successMessage: string | null;
  error: string | null;
  
  // Actions
  addToWatchlist: (movie: Movie) => void;
  removeFromWatchlistIds: (tmdbId: number) => void;
  isInWatchlist: (tmdbId: number) => boolean;
  setStatus: (status: WatchlistStatus) => void;
  setNotes: (notes: string) => void;
  handleCloseDialog: () => void;
  handleConfirmAdd: () => Promise<void>;
  refreshWatchlistIds: () => Promise<void>;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

export const WatchlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const [watchlistMovieIds, setWatchlistMovieIds] = useState<number[]>([]);
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Load user's watchlist movie IDs on mount and when user changes
  const refreshWatchlistIds = useCallback(async () => {
    if (!user?.id) {
      setWatchlistMovieIds([]);
      return;
    }

    try {
      const watchlist = await watchlistService.getUserWatchlist(user.id);
      const ids = watchlist.map(item => item.movie?.tmdbId).filter((id): id is number => id !== undefined);
      setWatchlistMovieIds(ids);
    } catch (err) {
      console.error('Failed to load watchlist IDs:', err);
    }
  }, [user?.id]);

  useEffect(() => {
    refreshWatchlistIds();
  }, [refreshWatchlistIds]);

  const addToWatchlist = useCallback((movie: Movie) => {
    if (!user) {
      setError('Please log in to add movies to your watchlist');
      setTimeout(() => setError(null), 5000);
      return;
    }
    setSelectedMovie(movie);
    setAddDialogOpen(true);
  }, [user]);

  const isInWatchlist = useCallback((tmdbId: number) => {
    return watchlistMovieIds.includes(tmdbId);
  }, [watchlistMovieIds]);

  const removeFromWatchlistIds = useCallback((tmdbId: number) => {
    setWatchlistMovieIds(prev => prev.filter(id => id !== tmdbId));
  }, []);

  const handleCloseDialog = useCallback(() => {
    setAddDialogOpen(false);
    setSelectedMovie(null);
    setStatus(WatchlistStatus.Planned);
    setNotes('');
  }, []);

  const handleConfirmAdd = useCallback(async () => {
    if (!selectedMovie || !user) return;

    try {
      await watchlistService.addToWatchlist(user.id, {
        movieId: selectedMovie.tmdbId,
        status,
        notes: notes.trim() || undefined,
      });

      setSuccessMessage(`"${selectedMovie.title}" added to your watchlist!`);
      handleCloseDialog();
      
      // Optimistically update watchlist IDs without making another API call
      setWatchlistMovieIds(prev => [...prev, selectedMovie.tmdbId]);

      // Auto-hide success message after 3 seconds
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to add movie to watchlist');
      handleCloseDialog();
      
      // Auto-hide error after 5 seconds
      setTimeout(() => setError(null), 5000);
    }
  }, [selectedMovie, user, status, notes, handleCloseDialog]);

  const value: WatchlistContextType = {
    watchlistMovieIds,
    selectedMovie,
    addDialogOpen,
    status,
    notes,
    successMessage,
    error,
    addToWatchlist,
    removeFromWatchlistIds,
    isInWatchlist,
    setStatus,
    setNotes,
    handleCloseDialog,
    handleConfirmAdd,
    refreshWatchlistIds,
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

