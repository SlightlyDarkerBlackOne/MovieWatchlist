import { useState, useEffect, useMemo } from 'react';
import { useGetPopularMoviesQuery } from '../store/api/moviesApi';
import { Movie } from '../types/movie.types';
import { CACHE_CONFIG } from '../utils/constants';

export const useFeaturedMovies = () => {
  const [featuredMovies, setFeaturedMovies] = useState<Movie[]>([]);

  const { 
    data: popularMovies, 
    isLoading: popularMoviesLoading, 
    error: popularMoviesError,
    refetch: refetchPopularMovies
  } = useGetPopularMoviesQuery(
    { page: 1 },
    { pollingInterval: CACHE_CONFIG.POPULAR_MOVIES_CACHE_MINUTES * 60 * 1000 }
  );

  useEffect(() => {
    if (popularMovies?.movies && popularMovies.movies.length > 0) {
      setFeaturedMovies(popularMovies.movies.slice(0, 5));
    }
  }, [popularMovies]);

  const featuredMovieIds = useMemo(() => {
    return featuredMovies.map(m => m.tmdbId);
  }, [featuredMovies]);

  return {
    featuredMovies,
    featuredMovieIds,
    popularMoviesLoading,
    popularMoviesError,
    refetchPopularMovies
  };
};

