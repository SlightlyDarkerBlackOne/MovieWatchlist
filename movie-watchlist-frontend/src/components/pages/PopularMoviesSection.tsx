import React from 'react';
import { Box, Typography, IconButton, Tooltip } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import { InfiniteMovieList } from '../movies';
import { useInfiniteMovies } from '../../hooks/useInfiniteMovies';

interface PopularMoviesSectionProps {
  onRefresh?: () => void;
}

const PopularMoviesSection: React.FC<PopularMoviesSectionProps> = ({ 
  onRefresh 
}) => {
  const { movies, loading, hasMore, loadMore, loadingMore } = useInfiniteMovies({ initialLimit: 5 });

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
      />
    </Box>
  );
};

export default PopularMoviesSection;

