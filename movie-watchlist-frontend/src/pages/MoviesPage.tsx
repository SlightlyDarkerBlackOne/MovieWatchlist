import React, { useState, useEffect, useRef } from 'react';
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
import * as movieService from '../services/movieService';
import { Movie, MovieSearchResult } from '../types/movie.types';
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
  const [searchResults, setSearchResults] = useState<MovieSearchResult | null>(null);
  const [popularMovies, setPopularMovies] = useState<MovieSearchResult | null>(null);
  const [featuredMovies, setFeaturedMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog state
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = useState(false);
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null);
  const [status, setStatus] = useState<WatchlistStatus>(WatchlistStatus.Planned);
  const [notes, setNotes] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

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
        movieId: selectedMovie.id,
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
        onClose={() => setError(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" variant="filled" sx={{ width: '100%' }} onClose={() => setError(null)}>
          {error}
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
          />
        )}

        {/* Popular Movies Section */}
        {!searchResults && (
          <PopularMoviesSection
            movies={popularMovies?.movies || []}
            loading={loading}
            onRefresh={handleRefreshPopularMovies}
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
