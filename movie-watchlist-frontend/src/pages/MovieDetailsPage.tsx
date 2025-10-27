import React, { useState, useEffect, useRef } from 'react';
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
import * as movieService from '../services/movieService';
import { MovieDetails, MovieVideo, MovieCredits } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useWatchlist } from '../contexts/WatchlistContext';
import { useAddToWatchlistOperation, useRemoveFromWatchlistOperation } from '../hooks/useWatchlistOperations';
import { useAuth } from '../contexts/AuthContext';
import { MovieDetailsContent, TrailerSection } from '../components/pages';
import { AddToWatchlistDialog } from '../components/dialogs';
import LoginRequiredDialog from '../components/common/LoginRequiredDialog';

const MovieDetailsPage: React.FC = () => {
  const { tmdbId } = useParams<{ tmdbId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { isInWatchlist } = useWatchlist();
  const { addToWatchlist } = useAddToWatchlistOperation();
  const { removeItem } = useRemoveFromWatchlistOperation();
  
  const [movieDetails, setMovieDetails] = useState<MovieDetails | null>(null);
  const [videos, setVideos] = useState<MovieVideo[]>([]);
  const [credits, setCredits] = useState<MovieCredits | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [showTrailer, setShowTrailer] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Dialog state
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = useState(false);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const trailerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (tmdbId) {
      loadMovieData(parseInt(tmdbId));
    }
  }, [tmdbId]);

  useEffect(() => {
    if (showTrailer && trailerRef.current) {
      requestAnimationFrame(() => {
        trailerRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      });
    }
  }, [showTrailer]);

  const loadMovieData = async (id: number) => {
    setLoading(true);
    setLoadError(null);
    
    try {
      const { movie, credits, videos } = await movieService.getMovieDetailsByTmdbId(id);
      console.log('Movie data received:', movie);
      console.log('Movie genres:', movie.genres);
      setMovieDetails(movie);
      setVideos(videos);
      setCredits(credits);
    } catch (err) {
      const error = err as Error;
      setLoadError(error.message || 'Failed to load movie details');
    } finally {
      setLoading(false);
    }
  };

  const handleAddToWatchlist = () => {
    if (!user) {
      setLoginRequiredDialogOpen(true);
      return;
    }
    setAddDialogOpen(true);
  };

  const handleConfirmAdd = async () => {
    if (!movieDetails || !user) return;
    
    try {
      await addToWatchlist(user.id, {
        movieId: movieDetails.id,
        status,
        notes
      });
      
      setSuccessMessage(`Added "${movieDetails.title}" to your watchlist!`);
      handleCloseDialog();
    } catch (err) {
      console.error('Failed to add to watchlist:', err);
      setActionError('Failed to add to watchlist');
    }
  };

  const handleRemoveFromWatchlist = async () => {
    if (!movieDetails || !user) return;
    
    // Find the watchlist item
    // This would need to query the watchlist to find the item
    // For now, we'll just show an error
    setActionError('Remove functionality needs watchlist item ID');
  };

  const handleToggleTrailer = () => {
    setShowTrailer(!showTrailer);
  };

  const handleCloseDialog = () => {
    setAddDialogOpen(false);
    setStatus(WatchlistStatus.Planned);
    setNotes('');
  };

  const handleCloseLoginDialog = () => {
    setLoginRequiredDialogOpen(false);
  };

  // Clear success message after timeout
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
        <Alert severity="error">{loadError || 'Movie not found'}</Alert>
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

  const mainTrailer = movieService.findMainTrailer(videos);
  const topCast = credits?.cast.slice(0, 10) || [];

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

      {/* Error Toast removed as we're using local state now */}

      {/* Main Movie Details */}
      <MovieDetailsContent
        movieDetails={movieDetails}
        videos={videos}
        credits={credits}
        showTrailer={showTrailer}
        onToggleTrailer={handleToggleTrailer}
        onAddToWatchlist={handleAddToWatchlist}
        onRemoveFromWatchlist={handleRemoveFromWatchlist}
        isInWatchlist={isInWatchlist(movieDetails.tmdbId)}
      />

      {/* Trailer Section */}
      <TrailerSection trailer={mainTrailer} show={showTrailer} />

      {/* Top Cast Section - rendered within MovieDetailsContent */}

      {/* Add to Watchlist Dialog */}
      <AddToWatchlistDialog
        open={addDialogOpen}
        onClose={handleCloseDialog}
        onConfirm={handleConfirmAdd}
        status={status}
        setStatus={setStatus}
        notes={notes}
        setNotes={setNotes}
        movieTitle={movieDetails?.title}
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

