import React from 'react';
import {
  Box,
  Typography,
  CircularProgress,
  Pagination,
  Grid2
} from '@mui/material';
import MovieCard from './MovieCard';
import { Movie } from '../../types/movie.types';

interface MovieListProps {
  movies: Movie[];
  loading?: boolean;
  currentPage?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
}

const MovieList: React.FC<MovieListProps> = ({
  movies,
  loading = false,
  currentPage = 1,
  totalPages = 1,
  onPageChange
}) => {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (movies.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary">
          No movies found
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Try searching for something else
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Movie Grid using Grid2 */}
      <Grid2 container spacing={3}>
        {movies.map((movie) => (
          <Grid2 key={movie.tmdbId} size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
            <MovieCard movie={movie} />
          </Grid2>
        ))}
      </Grid2>

      {/* Pagination */}
      {totalPages > 1 && onPageChange && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <Pagination
            count={totalPages}
            page={currentPage}
            onChange={(_, page) => onPageChange(page)}
            color="primary"
            size="large"
            showFirstButton
            showLastButton
          />
        </Box>
      )}
    </Box>
  );
};

export default MovieList;

