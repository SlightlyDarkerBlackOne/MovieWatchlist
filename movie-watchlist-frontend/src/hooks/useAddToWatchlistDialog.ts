import { useState, useCallback } from 'react';
import { Movie } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';

interface AddToWatchlistDialogState {
  open: boolean;
  selectedMovie: Movie | null;
  status: WatchlistStatus;
  notes: string;
}

export const useAddToWatchlistDialog = () => {
  const [state, setState] = useState<AddToWatchlistDialogState>({
    open: false,
    selectedMovie: null,
    status: WatchlistStatus.Planned,
    notes: ''
  });

  const open = useCallback((movie: Movie) => {
    setState({
      open: true,
      selectedMovie: movie,
      status: WatchlistStatus.Planned,
      notes: ''
    });
  }, []);

  const close = useCallback(() => {
    setState({
      open: false,
      selectedMovie: null,
      status: WatchlistStatus.Planned,
      notes: ''
    });
  }, []);

  const updateStatus = useCallback((status: WatchlistStatus) => {
    setState(prev => ({ ...prev, status }));
  }, []);

  const updateNotes = useCallback((notes: string) => {
    setState(prev => ({ ...prev, notes }));
  }, []);

  return {
    isOpen: state.open,
    selectedMovie: state.selectedMovie,
    status: state.status,
    notes: state.notes,
    openDialog: open,
    closeDialog: close,
    setStatus: updateStatus,
    setNotes: updateNotes
  };
};

