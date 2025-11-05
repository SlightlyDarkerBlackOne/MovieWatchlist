import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  CircularProgress,
  Alert,
  Button,
  Snackbar,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useGetMovieDetailsQuery } from '../store/api/moviesApi';
import { WatchlistStatus, AddToWatchlistRequest } from '../types/watchlist.types';
import { useWatchlistPresence } from '../hooks/useWatchlistPresence';
import { useAddToWatchlistMutation, useRemoveFromWatchlistMutation, useGetWatchlistQuery } from '../hooks/useWatchlistOperations';
import { useAddToWatchlistDialog } from '../hooks/useAddToWatchlistDialog';
import { useAuth } from '../contexts/AuthContext';
import MovieMainDetails from '../components/movies/MovieMainDetails';
import MovieGenres from '../components/movies/MovieGenres';
import TopCastCrew from '../components/movies/TopCastCrew';
import TrailerSection from '../components/pages/TrailerSection';
import { findMainTrailer } from '../services/movieService';
import { AddToWatchlistDialog } from '../components/dialogs';
import LoginRequiredDialog from '../components/common/LoginRequiredDialog';

const MovieDetailsPage: React.FC = () => {
  const { tmdbId } = useParams<{ tmdbId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [addToWatchlist] = useAddToWatchlistMutation();
  const [removeFromWatchlist] = useRemoveFromWatchlistMutation();
  const { data: watchlistItems } = useGetWatchlistQuery(undefined, { skip: !user });
  
  const {
    data: movieData,
    isLoading: loading,
    error: loadError
  } = useGetMovieDetailsQuery(
    { tmdbId: parseInt(tmdbId || '0') },
    { skip: !tmdbId }
  );

  const movieDetails = movieData?.movie;
  const { isInWatchlist: isMovieInWatchlist } = useWatchlistPresence(movieDetails?.tmdbId ?? 0);
  const videos = movieData?.videos || [];
  const credits = movieData?.credits || null;
  
  const [actionError, setActionError] = useState<string | null>(null);
  const [showTrailer, setShowTrailer] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = useState(false);
  const dialog = useAddToWatchlistDialog();

  const handleAddToWatchlist = () => {
    if (!user) {
      setLoginRequiredDialogOpen(true);
      return;
    }
    if (movieDetails) {
      dialog.openDialog(movieDetails);
    }
  };

  const handleConfirmAdd = async () => {
    if (!dialog.selectedMovie || !user) return;
    
    try {
      const request: AddToWatchlistRequest = {
        movieId: dialog.selectedMovie.tmdbId,
        status: dialog.status,
        notes: dialog.notes || undefined
      };
      
      await addToWatchlist(request).unwrap();
      
      setSuccessMessage(`Added "${dialog.selectedMovie.title}" to your watchlist!`);
      dialog.closeDialog();
    } catch (err) {
      const error = err as Error;
      setActionError(error.message || 'Failed to add to watchlist');
    }
  };

  const handleFormChange = useCallback((form: { status: WatchlistStatus; notes: string }) => {
    dialog.setStatus(form.status);
    dialog.setNotes(form.notes);
  }, [dialog]);

  const handleRemoveFromWatchlist = async () => {
    if (!movieDetails || !user || !watchlistItems) return;
    
    const watchlistItem = watchlistItems.find(item => item.movie?.tmdbId === movieDetails.tmdbId);
    
    if (!watchlistItem) {
      setActionError('Movie not found in watchlist');
      return;
    }
    
    try {
      await removeFromWatchlist(watchlistItem.id).unwrap();
      setSuccessMessage(`Removed "${movieDetails.title}" from your watchlist!`);
    } catch (err) {
      const error = err as Error;
      setActionError(error.message || 'Failed to remove from watchlist');
    }
  };

  const handleToggleTrailer = () => {
    setShowTrailer(!showTrailer);
  };

  const handleCloseLoginDialog = () => {
    setLoginRequiredDialogOpen(false);
  };

  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => setSuccessMessage(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '70vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (loadError || !movieDetails) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">{loadError ? String(loadError) : 'Movie not found'}</Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(-1)}
          sx={{ mt: 2 }}
        >
          Go Back
        </Button>
      </Container>
    );
  }


  return (
    <>
      {/* Success Toast */}
      <Snackbar
        open={!!successMessage}
        autoHideDuration={3000}
        onClose={() => setSuccessMessage(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" sx={{ width: '100%' }}>
          {successMessage}
        </Alert>
      </Snackbar>

      {/* Error Toasts */}
      <Snackbar
        open={!!actionError}
        autoHideDuration={5000}
        onClose={() => setActionError(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }} onClose={() => setActionError(null)}>
          {actionError}
        </Alert>
      </Snackbar>

      {/* Main Movie Details */}
      <MovieMainDetails
        movieDetails={movieDetails}
        videos={videos}
        credits={credits}
        showTrailer={showTrailer}
        onToggleTrailer={handleToggleTrailer}
        onAddToWatchlist={handleAddToWatchlist}
        onRemoveFromWatchlist={handleRemoveFromWatchlist}
        isInWatchlist={isMovieInWatchlist}
      />

      <MovieGenres genres={movieDetails.genres} />

      <TrailerSection trailer={findMainTrailer(videos)} show={showTrailer} />

      <TopCastCrew topCast={credits?.cast.slice(0, 10) || []} />

      {/* Add to Watchlist Dialog */}
      <AddToWatchlistDialog
        open={dialog.isOpen}
        onClose={dialog.closeDialog}
        onConfirm={handleConfirmAdd}
        form={{ status: dialog.status, notes: dialog.notes }}
        onChange={handleFormChange}
        movieTitle={dialog.selectedMovie?.title}
      />

      {/* Login Required Dialog */}
      <LoginRequiredDialog 
        open={loginRequiredDialogOpen} 
        onClose={handleCloseLoginDialog} 
      />
    </>
  );
};

export default MovieDetailsPage;

