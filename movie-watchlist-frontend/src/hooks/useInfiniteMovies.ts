import { useState, useCallback } from 'react';
import { useGetPopularMoviesInfiniteQuery } from '../store/api/moviesApi';
import { Movie } from '../types/movie.types';

interface UseInfiniteMoviesOptions {
  initialLimit?: number;
  excludeTmdbIds?: number[];
}

interface UseInfiniteMoviesReturn {
  movies: Movie[];
  loading: boolean;
  error: unknown;
  hasMore: boolean;
  loadMore: () => void;
  loadingMore: boolean;
}

export function useInfiniteMovies(options: UseInfiniteMoviesOptions = {}): UseInfiniteMoviesReturn {
  const { initialLimit = 5, excludeTmdbIds = [] } = options;
  const [limit, setLimit] = useState(initialLimit);
  
  const { data, isLoading, error } = useGetPopularMoviesInfiniteQuery({ limit });

  const allMovies = (data || [])
    .flatMap(page => page.movies)
    .filter(movie => !excludeTmdbIds.includes(movie.tmdbId));
  
  const hasMore = isLoading ? false : (data?.length || 0) > 0;

  const loadingMore = isLoading;

  const loadMore = useCallback(() => {
    if (!loadingMore) {
      setLimit(prev => prev + 1);
    }
  }, [loadingMore]);

  return {
    movies: allMovies,
    loading: isLoading,
    error,
    hasMore,
    loadMore,
    loadingMore
  };
}

