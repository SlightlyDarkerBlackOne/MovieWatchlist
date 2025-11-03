import React from 'react';
import { Box, Typography } from '@mui/material';
import { MovieList } from '../movies';
import ErrorState from '../ui/ErrorState';
import { Movie } from '../../types/movie.types';

interface SearchResultsProps {
  query: string;
  movies: Movie[];
  loading: boolean;
  error?: unknown;
  onRetry?: () => void;
  containerRef?: React.Ref<HTMLDivElement>;
  onAddToWatchlist?: (movie: Movie) => void;
}

const SearchResults: React.FC<SearchResultsProps> = ({ 
  query, 
  movies, 
  loading,
  error,
  onRetry,
  containerRef,
  onAddToWatchlist
}) => {
  if (error) {
    return (
      <Box ref={containerRef} sx={{ mb: 4, scrollMarginTop: '100px' }}>
        <ErrorState 
          message={`Failed to search for "${query}". ${error instanceof Error ? error.message : 'Please try again.'}`}
          onRetry={onRetry}
          retryLabel="Retry Search"
        />
      </Box>
    );
  }

  return (
    <Box ref={containerRef} sx={{ mb: 4, scrollMarginTop: '100px' }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4" component="h2" sx={{ fontWeight: 600 }}>
          Search Results for "{query}"
        </Typography>
      </Box>
      <MovieList movies={movies} loading={loading} onAddToWatchlist={onAddToWatchlist} />
      {movies.length === 0 && !loading && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="h6" color="text.secondary">
            No movies found matching "{query}"
          </Typography>
        </Box>
      )}
    </Box>
  );
};

export default SearchResults;

