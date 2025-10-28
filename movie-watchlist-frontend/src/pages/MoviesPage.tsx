import React, { useState, useEffect, useRef, useMemo } from 'react';
import {
  Container,
  Snackbar,
  Alert
} from '@mui/material';
import { useSearchParams } from 'react-router-dom';
import { FeaturedMoviesCarousel } from '../components/movies';
import { SearchResults, PopularMoviesSection } from '../components/pages';
import { AddToWatchlistDialog } from '../components/dialogs';
import LoginRequiredDialog from '../components/common/LoginRequiredDialog';
import { useSearchMoviesQuery, useGetPopularMoviesQuery } from '../store/api/moviesApi';
import { Movie } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useWatchlist } from '../contexts/WatchlistContext';
import { useAddToWatchlistOperation } from '../hooks/useWatchlistOperations';
import { useAuth } from '../contexts/AuthContext';

const MoviesPage: React.FC = () => {
  const { user } = useAuth();
  const { addToWatchlist, isLoading: watchlistLoading, error: watchlistError } = useAddToWatchlistOperation();
  const [searchParams] = useSearchParams();
  const searchResultsRef = useRef<HTMLDivElement>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [featuredMovies, setFeaturedMovies] = useState<Movie[]>([]);
  
  const { 
    data: popularMovies, 
    isLoading: popularMoviesLoading, 
    error: popularMoviesError,
    refetch: refetchPopularMovies
  } = useGetPopularMoviesQuery({ page: 1 });

  const { 
    data: searchResults, 
    isLoading: searchLoading, 
    error: searchError
  } = useSearchMoviesQuery(
    { query: searchQuery, page: 1 },
    { skip: !searchQuery.trim() }
  );
  
  // Dialog state
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = useState(false);
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const loading = popularMoviesLoading || searchLoading;
  const error = popularMoviesError || searchError;

  // Set featured movies from the first few popular movies
  useEffect(() => {
    if (popularMovies?.movies && popularMovies.movies.length > 0) {
      setFeaturedMovies(popularMovies.movies.slice(0, 5));
    }
  }, [popularMovies]);

  // Memoize featured movie IDs to prevent infinite loops
  const featuredMovieIds = useMemo(() => {
    return featuredMovies.map(m => m.tmdbId);
  }, [featuredMovies]);

  // Auto-refresh popular movies every 3 minutes
  useEffect(() => {
    const AUTO_REFRESH_INTERVAL = 3 * 60 * 1000;
    
    const intervalId = setInterval(() => {
      console.log('Auto-refreshing popular movies...');
      refetchPopularMovies();
    }, AUTO_REFRESH_INTERVAL);

    return () => clearInterval(intervalId);
  }, [refetchPopularMovies]);

  // Handle search from URL parameters
  useEffect(() => {
    const urlSearchQuery = searchParams.get('search');
    if (urlSearchQuery && urlSearchQuery !== searchQuery) {
      setSearchQuery(urlSearchQuery);
    }
  }, [searchParams]);

  // Scroll to search results when they load
  useEffect(() => {
    if (searchResults && searchQuery) {
      setTimeout(() => {
        searchResultsRef.current?.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'start' 
        });
      }, 100);
    }
  }, [searchResults, searchQuery]);

  const handleRefreshPopularMovies = () => {
    refetchPopularMovies();
  };

  // Watchlist handlers
  const handleAddToWatchlist = (movie: Movie) => {
    if (!user) {
      setLoginRequiredDialogOpen(true);
      return;
    }
    setSelectedMovie(movie);
    setAddDialogOpen(true);
  };

  const handleConfirmAdd = async () => {
    if (!selectedMovie || !user) return;
    
    try {
      await addToWatchlist(user.id, {
        movieId: selectedMovie.tmdbId,
        status,
        notes
      });
      
      setSuccessMessage(`Added "${selectedMovie.title}" to your watchlist!`);
      handleCloseDialog();
    } catch (err) {
      console.error('Failed to add to watchlist:', err);
    }
  };

  const handleCloseDialog = () => {
    setAddDialogOpen(false);
    setSelectedMovie(null);
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

  return (
    <>
      {/* Success Toast */}
      <Snackbar
        open={!!successMessage}
        autoHideDuration={3000}
        onClose={() => {}}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" sx={{ width: '100%' }}>
          {successMessage}
        </Alert>
      </Snackbar>

      {/* Error Toasts */}
      <Snackbar
        open={!!error}
        autoHideDuration={5000}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }}>
          {error ? String(error) : 'An error occurred'}
        </Alert>
      </Snackbar>

      <Snackbar
        open={!!watchlistError}
        autoHideDuration={5000}
        onClose={() => {}}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }}>
          {watchlistError ? String(watchlistError) : 'An error occurred'}
        </Alert>
      </Snackbar>

      {/* Featured Movies Carousel */}
      {featuredMovies.length > 0 && (
        <FeaturedMoviesCarousel
          movies={featuredMovies}
          onAddToWatchlist={handleAddToWatchlist}
          autoRotate={true}
          rotateInterval={5000}
        />
      )}

      {/* Main Content */}
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Search Results Section */}
        {searchResults && (
          <SearchResults
            query={searchQuery}
            movies={searchResults.movies}
            loading={loading}
            containerRef={searchResultsRef}
            onAddToWatchlist={handleAddToWatchlist}
          />
        )}

        {/* Popular Movies Section */}
        {!searchResults && (
          <PopularMoviesSection
            onRefresh={handleRefreshPopularMovies}
            excludeTmdbIds={featuredMovieIds}
            onAddToWatchlist={handleAddToWatchlist}
          />
        )}
      </Container>

      {/* Add to Watchlist Dialog */}
      <AddToWatchlistDialog
        open={addDialogOpen}
        onClose={handleCloseDialog}
        onConfirm={handleConfirmAdd}
        status={status}
        setStatus={setStatus}
        notes={notes}
        setNotes={setNotes}
        movieTitle={selectedMovie?.title}
      />

      {/* Login Required Dialog */}
      <LoginRequiredDialog 
        open={loginRequiredDialogOpen} 
        onClose={handleCloseLoginDialog} 
      />
    </>
  );
};

export default MoviesPage;
