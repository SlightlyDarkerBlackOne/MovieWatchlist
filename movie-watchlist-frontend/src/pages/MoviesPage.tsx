import React, { useCallback } from 'react';
import { Container } from '@mui/material';
import { FeaturedMoviesCarousel } from '../components/movies';
import { SearchResults, PopularMoviesSection } from '../components/pages';
import { AddToWatchlistDialog } from '../components/dialogs';
import LoginRequiredDialog from '../components/common/LoginRequiredDialog';
import SuccessToast from '../components/ui/SuccessToast';
import { Movie } from '../types/movie.types';
import { WatchlistStatus } from '../types/watchlist.types';
import { useAddToWatchlistMutation } from '../hooks/useWatchlistOperations';
import { useAuth } from '../contexts/AuthContext';
import { useMovieSearch } from '../hooks/useMovieSearch';
import { useFeaturedMovies } from '../hooks/useFeaturedMovies';
import { useAddToWatchlistDialog } from '../hooks/useAddToWatchlistDialog';
import { useSuccessToast } from '../hooks/useSuccessToast';

const MoviesPage: React.FC = () => {
  const { user } = useAuth();
  const [addToWatchlist] = useAddToWatchlistMutation();
  const [loginRequiredDialogOpen, setLoginRequiredDialogOpen] = React.useState(false);
  const successToast = useSuccessToast();
  
  const { 
    searchQuery, 
    searchResults, 
    searchLoading, 
    searchError,
    searchResultsRef,
    setSearchQuery
  } = useMovieSearch();

  const { 
    featuredMovies,
    featuredMovieIds,
    popularMoviesError,
    refetchPopularMovies
  } = useFeaturedMovies();

  const dialog = useAddToWatchlistDialog();

  const handleAddToWatchlist = useCallback((movie: Movie) => {
    if (!user) {
      setLoginRequiredDialogOpen(true);
      return;
    }
    dialog.openDialog(movie);
  }, [user, dialog]);

  const handleConfirmAdd = useCallback(async () => {
    if (!dialog.selectedMovie || !user) return;
    
    try {
      await addToWatchlist({
        movieId: dialog.selectedMovie.tmdbId,
        status: dialog.status,
        notes: dialog.notes || undefined
      }).unwrap();
      
      successToast.showMessage(`Added "${dialog.selectedMovie.title}" to your watchlist!`);
      dialog.closeDialog();
    } catch {
      successToast.hideMessage();
    }
  }, [dialog.selectedMovie, dialog.status, dialog.notes, user, addToWatchlist, dialog, successToast]);

  const handleCloseLoginDialog = useCallback(() => {
    setLoginRequiredDialogOpen(false);
  }, []);


  const handleFormChange = useCallback((form: { status: WatchlistStatus; notes: string }) => {
    dialog.setStatus(form.status);
    dialog.setNotes(form.notes);
  }, [dialog]);

  const handleRetrySearch = useCallback(() => {
    setSearchQuery(searchQuery);
  }, [searchQuery, setSearchQuery]);

  return (
    <>
      <SuccessToast message={successToast.message} onClose={successToast.hideMessage} />

      {featuredMovies.length > 0 && (
        <FeaturedMoviesCarousel
          movies={featuredMovies}
          onAddToWatchlist={handleAddToWatchlist}
          autoRotate={true}
          rotateInterval={5000}
        />
      )}

      <Container maxWidth="xl" sx={{ py: 4 }}>
        {searchResults && (
          <SearchResults
            query={searchQuery}
            movies={searchResults.movies}
            loading={searchLoading}
            error={searchError}
            onRetry={handleRetrySearch}
            containerRef={searchResultsRef}
            onAddToWatchlist={handleAddToWatchlist}
          />
        )}

        {!searchResults && (
          <PopularMoviesSection
            onRefresh={refetchPopularMovies}
            excludeTmdbIds={featuredMovieIds}
            error={popularMoviesError}
            onRetry={refetchPopularMovies}
            onAddToWatchlist={handleAddToWatchlist}
          />
        )}
      </Container>

      <AddToWatchlistDialog
        open={dialog.isOpen}
        onClose={dialog.closeDialog}
        onConfirm={handleConfirmAdd}
        form={{ status: dialog.status, notes: dialog.notes }}
        onChange={handleFormChange}
        movieTitle={dialog.selectedMovie?.title}
      />

      <LoginRequiredDialog 
        open={loginRequiredDialogOpen} 
        onClose={handleCloseLoginDialog} 
      />
    </>
  );
};

export default MoviesPage;
