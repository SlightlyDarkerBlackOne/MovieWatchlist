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
import { useGetMovieDetailsQuery } from '../features/movies/api/moviesApi';
import { WatchlistStatus, AddToWatchlistRequest } from '../features/watchlist/model/watchlist.types';
import { useWatchlistPresence } from '../features/watchlist/hooks/useWatchlistPresence';
import { useAddToWatchlistMutation, useRemoveFromWatchlistMutation, useGetWatchlistQuery } from '../features/watchlist/api/watchlistApi';
import { useAddToWatchlistDialog } from '../features/watchlist/hooks/useAddToWatchlistDialog';
import { useAuth } from '../features/auth/contexts/AuthContext';
import MovieMainDetails from '../features/movies/components/MovieMainDetails';
import MovieGenres from '../features/movies/components/MovieGenres';
import TopCastCrew from '../features/movies/components/TopCastCrew';
import TrailerSection from '../features/movies/components/TrailerSection';
import { findMainTrailer } from '../features/movies/lib/tmdbUtils';
import { AddToWatchlistDialog } from '../features/watchlist/components';
import LoginRequiredDialog from '../shared/components/common/LoginRequiredDialog';
import {
  ERROR_MESSAGES,
  SUCCESS_MESSAGES,
  UI_CONSTANTS,
  MOVIE_DETAILS_PAGE_TEXT,
  DEFAULT_VALUES,
} from '../shared/constants/appConstants';
import { getErrorMessage } from '../shared/lib/errorHandler';

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
    { tmdbId: parseInt(tmdbId || String(DEFAULT_VALUES.TMDB_ID)) },
    { skip: !tmdbId }
  );

  const movieDetails = movieData?.movie;
  const { isInWatchlist: isMovieInWatchlist } = useWatchlistPresence(movieDetails?.tmdbId ?? DEFAULT_VALUES.TMDB_ID);
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
      
      setSuccessMessage(SUCCESS_MESSAGES.ADDED_TO_WATCHLIST(dialog.selectedMovie.title));
      dialog.closeDialog();
    } catch (err) {
      setActionError(getErrorMessage(err) || ERROR_MESSAGES.FAILED_TO_ADD_TO_WATCHLIST);
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
      setActionError(ERROR_MESSAGES.MOVIE_NOT_FOUND_IN_WATCHLIST);
      return;
    }
    
    try {
      await removeFromWatchlist(watchlistItem.id).unwrap();
      setSuccessMessage(SUCCESS_MESSAGES.REMOVED_FROM_WATCHLIST(movieDetails.title));
    } catch (err) {
      setActionError(getErrorMessage(err) || ERROR_MESSAGES.FAILED_TO_REMOVE_FROM_WATCHLIST);
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
      const timer = setTimeout(() => setSuccessMessage(null), UI_CONSTANTS.TIMEOUT.SUCCESS_MESSAGE);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '70vh' }}>
        <CircularProgress size={UI_CONSTANTS.LOADING_SPINNER.SIZE} />
      </Box>
    );
  }

  if (loadError || !movieDetails) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity={UI_CONSTANTS.ALERT_SEVERITY.ERROR}>{loadError ? getErrorMessage(loadError) : ERROR_MESSAGES.MOVIE_NOT_FOUND}</Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(-1)}
          sx={{ mt: 2 }}
        >
          {MOVIE_DETAILS_PAGE_TEXT.GO_BACK}
        </Button>
      </Container>
    );
  }


  return (
    <>
      {/* Success Toast */}
      <Snackbar
        open={!!successMessage}
        autoHideDuration={UI_CONSTANTS.SNACKBAR.SUCCESS_AUTO_HIDE_DURATION}
        onClose={() => setSuccessMessage(null)}
        anchorOrigin={{ 
          vertical: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.VERTICAL_TOP, 
          horizontal: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.HORIZONTAL_CENTER 
        }}
      >
        <Alert severity={UI_CONSTANTS.ALERT_SEVERITY.SUCCESS} variant="filled" sx={{ width: '100%' }}>
          {successMessage}
        </Alert>
      </Snackbar>

      {/* Error Toasts */}
      <Snackbar
        open={!!actionError}
        autoHideDuration={UI_CONSTANTS.SNACKBAR.AUTO_HIDE_DURATION}
        onClose={() => setActionError(null)}
        anchorOrigin={{ 
          vertical: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.VERTICAL_TOP, 
          horizontal: UI_CONSTANTS.SNACKBAR.ANCHOR_ORIGIN.HORIZONTAL_CENTER 
        }}
      >
        <Alert severity={UI_CONSTANTS.ALERT_SEVERITY.ERROR} variant="filled" sx={{ width: '100%' }} onClose={() => setActionError(null)}>
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

      <TopCastCrew topCast={credits?.cast.slice(0, UI_CONSTANTS.CAST_SLICE.TOP_COUNT) || []} />

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

