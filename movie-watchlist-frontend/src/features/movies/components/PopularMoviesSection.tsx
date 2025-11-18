import React from 'react';
import { Box, Typography, IconButton, Tooltip } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import InfiniteMovieList from './InfiniteMovieList';
import ErrorState from '../../../shared/components/ui/ErrorState';
import { useInfiniteMovies } from '../hooks/useInfiniteMovies';
import { getErrorMessage } from '../../../shared/lib/errorHandler';

interface PopularMoviesSectionProps {
  onRefresh?: () => void;
  excludeTmdbIds?: number[];
  error?: unknown;
  onRetry?: () => void;
  onAddToWatchlist?: (movie: import('../model/movie.types').Movie) => void;
}

const PopularMoviesSection: React.FC<PopularMoviesSectionProps> = ({ 
  onRefresh,
  excludeTmdbIds = [],
  error,
  onRetry,
  onAddToWatchlist
}) => {
  const { movies, loading, hasMore, loadMore, loadingMore } = useInfiniteMovies({ 
    initialLimit: 5,
    excludeTmdbIds 
  });

  if (error && movies.length === 0) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h4" component="h2" sx={{ fontWeight: 600 }}>
            Popular Movies
          </Typography>
        </Box>
        <ErrorState 
          message={`Failed to load popular movies. ${getErrorMessage(error)}`}
          onRetry={onRetry || onRefresh}
          retryLabel="Retry"
        />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4" component="h2" sx={{ fontWeight: 600 }}>
          Popular Movies
        </Typography>
        {onRefresh && (
          <Tooltip title="Refresh popular movies">
            <span>
              <IconButton 
                onClick={onRefresh} 
                disabled={loading}
                color="primary"
                aria-label="Refresh popular movies"
              >
                <RefreshIcon />
              </IconButton>
            </span>
          </Tooltip>
        )}
      </Box>
      <InfiniteMovieList 
        movies={movies}
        loading={loadingMore}
        hasMore={hasMore}
        onLoadMore={loadMore}
        loadingMessage="Loading more movies..."
        emptyMessage="No popular movies available"
        onAddToWatchlist={onAddToWatchlist}
      />
    </Box>
  );
};

export default PopularMoviesSection;

