import React, { useEffect, useRef } from 'react';
import { Box, CircularProgress, Typography, Grid2 } from '@mui/material';
import MovieCard from './MovieCard';
import { Movie } from '../../types/movie.types';

interface InfiniteMovieListProps {
  movies: Movie[];
  loading?: boolean;
  hasMore?: boolean;
  onLoadMore?: () => void;
  loadingMessage?: string;
  emptyMessage?: string;
  onAddToWatchlist?: (movie: Movie) => void;
}

const InfiniteMovieList: React.FC<InfiniteMovieListProps> = ({
  movies,
  loading = false,
  hasMore = false,
  onLoadMore,
  loadingMessage = 'Loading more movies...',
  emptyMessage = 'No movies found',
  onAddToWatchlist
}) => {
  const observerTarget = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!onLoadMore || !hasMore || loading) {
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          onLoadMore();
        }
      },
      { threshold: 0.1 }
    );

    const currentTarget = observerTarget.current;
    if (currentTarget) {
      observer.observe(currentTarget);
    }

    return () => {
      if (currentTarget) {
        observer.unobserve(currentTarget);
      }
    };
  }, [hasMore, loading, onLoadMore]);

  if (movies.length === 0 && !loading) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary">
          {emptyMessage}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Try searching for something else
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Grid2 container spacing={3}>
        {movies.map((movie, index) => (
          <Grid2 key={`${movie.tmdbId}-${index}`} size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
            <MovieCard movie={movie} onAddToWatchlist={onAddToWatchlist} />
          </Grid2>
        ))}
      </Grid2>

      {loading && (
        <Box 
          sx={{ display: 'flex', justifyContent: 'center', py: 4 }}
          role="status"
          aria-live="polite"
          aria-label={loadingMessage}
        >
          <CircularProgress aria-hidden="true" />
          <span className="sr-only">{loadingMessage}</span>
        </Box>
      )}

      {!loading && hasMore && (
        <Box ref={observerTarget} sx={{ minHeight: '20px', py: 2 }} aria-hidden="true" />
      )}

      {!hasMore && movies.length > 0 && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography variant="body2" color="text.secondary">
            All movies loaded
          </Typography>
        </Box>
      )}
    </Box>
  );
};

export default InfiniteMovieList;

