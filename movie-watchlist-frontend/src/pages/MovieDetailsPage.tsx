import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button,
  Typography,
  Snackbar,
} from '@mui/material';
import movieService from '../services/movieService';
import watchlistService from '../services/watchlistService';
import { MovieDetails, MovieVideo, MovieCredits } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from '../contexts/AuthContext';
import { useWatchlist } from '../contexts/WatchlistContext';
import { colors } from '../theme';
import MovieMainDetails from '../components/movies/MovieMainDetails';
import MovieGenres from '../components/movies/MovieGenres';
import TopCastCrew from '../components/movies/TopCastCrew';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';

const MovieDetailsPage: React.FC = () => {
  const { tmdbId } = useParams<{ tmdbId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const {
    addToWatchlist,
    removeFromWatchlist,
    successMessage: watchlistSuccessMessage,
    error: watchlistError,
    addDialogOpen,
    selectedMovie,
    status,
    notes,
    setStatus,
    setNotes,
    handleCloseDialog,
    handleConfirmAdd,
    isInWatchlist
  } = useWatchlist();
  
  const [movieDetails, setMovieDetails] = useState<MovieDetails | null>(null);
  const [videos, setVideos] = useState<MovieVideo[]>([]);
  const [credits, setCredits] = useState<MovieCredits | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null); // Error loading movie data
  const [actionError, setActionError] = useState<string | null>(null); // Error from user actions
  const [showTrailer, setShowTrailer] = useState(false);

  useEffect(() => {
    if (tmdbId) {
      loadMovieData(parseInt(tmdbId));
    }
  }, [tmdbId]);

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
    if (!movieDetails) return;
    addToWatchlist(movieDetails);
  };

  const handleRemoveFromWatchlist = async () => {
    if (!movieDetails) return;
    await removeFromWatchlist(movieDetails.tmdbId);
  };

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
        open={!!watchlistSuccessMessage}
        autoHideDuration={3000}
        onClose={() => {}}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" sx={{ width: '100%' }}>
          {watchlistSuccessMessage}
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

      <Snackbar
        open={!!watchlistError}
        autoHideDuration={5000}
        onClose={() => {}}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }}>
          {watchlistError}
        </Alert>
      </Snackbar>

      {/* Main Movie Details */}
      <MovieMainDetails
        movieDetails={movieDetails}
        videos={videos}
        credits={credits}
        showTrailer={showTrailer}
        onToggleTrailer={() => setShowTrailer(!showTrailer)}
        onAddToWatchlist={handleAddToWatchlist}
        onRemoveFromWatchlist={handleRemoveFromWatchlist}
        isInWatchlist={isInWatchlist(movieDetails.tmdbId)}
      />

      {/* Genres Section */}
      <MovieGenres genres={movieDetails.genres} />

      {/* Trailer Section */}
      {showTrailer && mainTrailer && (
        <Container maxWidth="xl" sx={{ py: 4 }}>
          <Box sx={{ position: 'relative', paddingBottom: '56.25%', height: 0, overflow: 'hidden' }}>
            <iframe
              src={movieService.getYouTubeEmbedUrl(mainTrailer.key)}
              title={mainTrailer.name}
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                border: 'none',
              }}
            />
          </Box>
        </Container>
      )}

      {/* Top Cast Section */}
      <TopCastCrew topCast={topCast} />

      {/* Add to Watchlist Dialog */}
      <Dialog open={addDialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          Add to Watchlist
          {movieDetails && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {movieDetails.title}
            </Typography>
          )}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <FormControl fullWidth>
              <InputLabel id="status-label">Status</InputLabel>
              <Select
                labelId="status-label"
                value={status}
                label="Status"
                onChange={(e) => setStatus(e.target.value as WatchlistStatus)}
              >
                <MenuItem value={WatchlistStatus.Planned}>Planned</MenuItem>
                <MenuItem value={WatchlistStatus.Watching}>Watching</MenuItem>
                <MenuItem value={WatchlistStatus.Watched}>Watched</MenuItem>
                <MenuItem value={WatchlistStatus.Dropped}>Dropped</MenuItem>
              </Select>
            </FormControl>

            <TextField
              label="Notes (optional)"
              multiline
              rows={3}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Add your thoughts about this movie..."
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleConfirmAdd} variant="contained" color="primary">
            Add to Watchlist
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default MovieDetailsPage;

