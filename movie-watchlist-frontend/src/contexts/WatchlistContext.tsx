import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { Movie } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from './AuthContext';
import watchlistService from '../services/watchlistService';

export interface WatchlistContextType {
  watchlistMovieIds: number[];
  selectedMovie: Movie | null;
  addDialogOpen: boolean;
  loginRequiredDialogOpen: boolean;
  status: WatchlistStatus;
  notes: string;
  successMessage: string | null;
  error: string | null;
  
  addToWatchlist: (movie: Movie) => void;
  removeFromWatchlist: (tmdbId: number) => Promise<void>;
  removeFromWatchlistIds: (tmdbId: number) => void;
  isInWatchlist: (tmdbId: number) => boolean;
  setStatus: (status: WatchlistStatus) => void;
  setNotes: (notes: string) => void;
  handleCloseDialog: () => void;
  handleCloseLoginDialog: () => void;
  handleConfirmAdd: () => Promise<void>;
  refreshWatchlistIds: () => Promise<void>;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

export const WatchlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const [watchlistMovieIds, setWatchlistMovieIds] = useState<number[]>([]);
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = useState(false);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Load user's watchlist movie IDs - called manually when needed
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

  // Load watchlist IDs when user logs in
  useEffect(() => {
    if (user?.id) {
      refreshWatchlistIds();
    } else {
      setWatchlistMovieIds([]);
    }
  }, [user?.id, refreshWatchlistIds]);

  const addToWatchlist = useCallback((movie: Movie) => {
    if (!user) {
      setLoginRequiredDialogOpen(true);
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

  const handleCloseLoginDialog = useCallback(() => {
    setLoginRequiredDialogOpen(false);
  }, []);

  const removeFromWatchlist = useCallback(async (tmdbId: number) => {
    if (!user) return;

    try {
      const watchlist = await watchlistService.getUserWatchlist(user.id);
      const itemToRemove = watchlist.find(item => item.movie?.tmdbId === tmdbId);
      
      if (!itemToRemove) {
        throw new Error('Movie not found in watchlist');
      }

      await watchlistService.removeFromWatchlist(user.id, itemToRemove.id);
      
      // Optimistically update watchlist IDs
      setWatchlistMovieIds(prev => prev.filter(id => id !== tmdbId));
      
      setSuccessMessage('Removed from your watchlist!');
      
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to remove from watchlist');
      
      setTimeout(() => setError(null), 5000);
    }
  }, [user]);

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
    loginRequiredDialogOpen,
    status,
    notes,
    successMessage,
    error,
    addToWatchlist,
    removeFromWatchlist,
    removeFromWatchlistIds,
    isInWatchlist,
    setStatus,
    setNotes,
    handleCloseDialog,
    handleCloseLoginDialog,
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

