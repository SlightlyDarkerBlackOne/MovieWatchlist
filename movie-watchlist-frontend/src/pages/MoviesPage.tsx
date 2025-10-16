import React, { useState, useEffect, useRef } from 'react';
import {
  Container,
  Box,
  Typography,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  IconButton,
  Tooltip,
  Divider
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useSearchParams } from 'react-router-dom';
import { MovieList, FeaturedMoviesCarousel } from '../components/movies';
import movieService from '../services/movieService';
import watchlistService from '../services/watchlistService';
import { Movie, MovieSearchResult } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useAuth } from '../contexts/AuthContext';
import { useWatchlist } from '../contexts/WatchlistContext';

const MoviesPage: React.FC = () => {
  const { user } = useAuth();
  const { 
    addToWatchlist,
    successMessage,
    error: watchlistError,
    addDialogOpen,
    selectedMovie,
    status,
    notes,
    setStatus,
    setNotes,
    handleCloseDialog,
    handleConfirmAdd
  } = useWatchlist();
  
  const [searchParams] = useSearchParams();
  const searchResultsRef = useRef<HTMLDivElement>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<MovieSearchResult | null>(null);
  const [popularMovies, setPopularMovies] = useState<MovieSearchResult | null>(null);
  const [featuredMovies, setFeaturedMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load popular movies and featured movies on mount
  useEffect(() => {
    loadPopularMovies();
  }, []);

  // Set featured movies from the first few popular movies
  useEffect(() => {
    if (popularMovies?.movies && popularMovies.movies.length > 0) {
      // Take top 5 movies for featured carousel
      setFeaturedMovies(popularMovies.movies.slice(0, 5));
    }
  }, [popularMovies]);

  // Auto-refresh popular movies every 3 minutes
  useEffect(() => {
    const AUTO_REFRESH_INTERVAL = 3 * 60 * 1000; // 3 minutes in milliseconds
    
    const intervalId = setInterval(() => {
      console.log('Auto-refreshing popular movies...');
      loadPopularMovies(popularMovies?.currentPage || 1, true);
    }, AUTO_REFRESH_INTERVAL);

    return () => clearInterval(intervalId);
  }, [popularMovies?.currentPage]);

  // Handle search from URL parameters
  useEffect(() => {
    const searchQuery = searchParams.get('search');
    if (searchQuery) {
      handleSearch(searchQuery);
    }
  }, [searchParams]);

  const loadPopularMovies = async (page: number = 1, forceRefresh: boolean = false) => {
    setLoading(true);
    setError(null);
    try {
      // Clear cache if force refresh is requested
      if (forceRefresh) {
        movieService.clearPopularMoviesCache();
      }
      
      const result = await movieService.getPopularMovies(page);
      setPopularMovies(result);
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to load popular movies');
    } finally {
      setLoading(false);
    }
  };

  const handleRefreshPopularMovies = () => {
    loadPopularMovies(popularMovies?.currentPage || 1, true);
  };

  const handleSearch = async (query: string, page: number = 1) => {
    if (!query.trim()) {
      setSearchResults(null);
      setSearchQuery('');
      return;
    }

    setLoading(true);
    setError(null);
    setSearchQuery(query);
    
    try {
      const result = await movieService.searchMovies(query, page);
      setSearchResults(result);
      
      // Scroll to search results after they're loaded
      setTimeout(() => {
        searchResultsRef.current?.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'start' 
        });
      }, 100);
    } catch (err) {
      const error = err as Error;
      setError(error.message || 'Failed to search movies');
    } finally {
      setLoading(false);
    }
  };

  // All watchlist handling moved to WatchlistContext

  return (
    <>
      {/* Featured Movies Carousel */}
      {featuredMovies.length > 0 && (
        <FeaturedMoviesCarousel
          movies={featuredMovies}
          onAddToWatchlist={addToWatchlist}
          autoRotate={true}
          rotateInterval={5000}
        />
      )}

      {/* Main Content */}
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Success Message */}
        {successMessage && (
          <Alert severity="success" sx={{ mb: 3 }}>
            {successMessage}
          </Alert>
        )}

        {/* Error Messages */}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}
        {watchlistError && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {watchlistError}
          </Alert>
        )}

        {/* Search Results Section */}
        {searchResults && (
          <Box ref={searchResultsRef} sx={{ mb: 4, scrollMarginTop: '100px' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
              <Typography variant="h4" component="h2" sx={{ fontWeight: 600 }}>
                Search Results for "{searchQuery}"
              </Typography>
            </Box>
            <MovieList
              movies={searchResults.movies}
              loading={false}
            />
            {searchResults.movies.length === 0 && !loading && (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="h6" color="text.secondary">
                  No movies found matching "{searchQuery}"
                </Typography>
              </Box>
            )}
          </Box>
        )}

        {/* Popular Movies Section */}
        {!searchResults && (
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
              <Typography variant="h4" component="h2" sx={{ fontWeight: 600 }}>
                Popular Movies
              </Typography>
              <Tooltip title="Refresh popular movies (clear cache)">
                <span>
                  <IconButton 
                    onClick={handleRefreshPopularMovies} 
                    disabled={loading}
                    color="primary"
                    aria-label="Refresh popular movies (clear cache)"
                  >
                    <RefreshIcon />
                  </IconButton>
                </span>
              </Tooltip>
            </Box>
            <MovieList
              movies={popularMovies?.movies || []}
              loading={false}
            />
          </Box>
        )}
      </Container>

      {/* Add to Watchlist Dialog */}
      <Dialog open={addDialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          Add to Watchlist
          {selectedMovie && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {selectedMovie.title}
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

export default MoviesPage;
