import api from './api';
import { API_ENDPOINTS } from '../utils/constants';
import { MovieSearchResult, MovieDetails, MovieVideo, MovieCredits } from '../types/movie.types';
import { transformCastMember, transformCrewMember, transformVideo } from '../utils/tmdbTransformers';
import { AxiosError } from '../types/error.types';

export const TMDB_IMAGE_CONFIG = {
  BASE_URL: 'https://image.tmdb.org/t/p',
  POSTER_SIZES: {
    SMALL: 'w185',
    MEDIUM: 'w342',
    LARGE: 'w500',
    ORIGINAL: 'original',
  },
  BACKDROP_SIZES: {
    SMALL: 'w300',
    MEDIUM: 'w780',
    LARGE: 'w1280',
    ORIGINAL: 'original',
  },
  PROFILE_SIZES: {
    SMALL: 'w185',
    MEDIUM: 'h632',
    LARGE: 'original',
  },
  PLACEHOLDER: '/placeholder-movie.png',
} as const;

export async function searchMovies(query: string, page: number = 1): Promise<MovieSearchResult> {
  try {
    const response = await api.get(API_ENDPOINTS.MOVIES.SEARCH, {
      params: { query, page }
    });
    
    const movies = Array.isArray(response.data) ? response.data : [];
    return {
      movies,
      totalResults: movies.length,
      totalPages: 1,
      currentPage: page
    };
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Movie search error:', axiosError);
    throw new Error(axiosError.response?.data?.message || 'Failed to search movies');
  }
}

export async function getPopularMovies(page: number = 1): Promise<MovieSearchResult> {
  try {
    const response = await api.get(API_ENDPOINTS.MOVIES.POPULAR, {
      params: { page }
    });
    
    const movies = Array.isArray(response.data) ? response.data : [];
    return {
      movies,
      totalResults: movies.length,
      totalPages: 1,
      currentPage: page
    };
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get popular movies error:', axiosError);
    throw new Error(axiosError.response?.data?.message || 'Failed to get popular movies');
  }
}

export async function getMoviesByGenre(genre: string, page: number = 1): Promise<MovieSearchResult> {
  try {
    const response = await api.get(API_ENDPOINTS.MOVIES.BY_GENRE(genre), {
      params: { page }
    });
    
    const movies = Array.isArray(response.data) ? response.data : [];
    return {
      movies,
      totalResults: movies.length,
      totalPages: 1,
      currentPage: page
    };
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get movies by genre error:', error);
    throw new Error(axiosError.response?.data?.message || 'Failed to get movies by genre');
  }
}

export async function getMovieDetailsByTmdbId(tmdbId: number): Promise<{
  movie: MovieDetails;
  credits: MovieCredits;
  videos: MovieVideo[];
}> {
  try {
    const response = await api.get(`/Movies/tmdb/${tmdbId}`);
    const data = response.data;
    
    const transformedCredits = {
      cast: (data.credits?.cast || []).map(transformCastMember),
      crew: (data.credits?.crew || []).map(transformCrewMember)
    };
    
    const transformedVideos = (data.videos || []).map(transformVideo);
    
    return {
      movie: data.movie,
      credits: transformedCredits,
      videos: transformedVideos
    };
  } catch (error) {
    const axiosError = error as AxiosError;
    console.error('Get movie details error:', axiosError);
    throw new Error(axiosError.response?.data?.message || 'Failed to get movie details');
  }
}

export function getPosterUrl(
  posterPath: string | null, 
  size: 'small' | 'medium' | 'large' | 'original' = 'medium'
): string | null {
  if (!posterPath) {
    return TMDB_IMAGE_CONFIG.PLACEHOLDER;
  }
  
  const sizeMap = {
    small: TMDB_IMAGE_CONFIG.POSTER_SIZES.SMALL,
    medium: TMDB_IMAGE_CONFIG.POSTER_SIZES.MEDIUM,
    large: TMDB_IMAGE_CONFIG.POSTER_SIZES.LARGE,
    original: TMDB_IMAGE_CONFIG.POSTER_SIZES.ORIGINAL,
  };
  
  return `${TMDB_IMAGE_CONFIG.BASE_URL}/${sizeMap[size]}${posterPath}`;
}

export function getBackdropUrl(
  backdropPath: string | null, 
  size: 'small' | 'medium' | 'large' | 'original' = 'medium'
): string | null {
  if (!backdropPath) {
    return null;
  }
  
  const sizeMap = {
    small: TMDB_IMAGE_CONFIG.BACKDROP_SIZES.SMALL,
    medium: TMDB_IMAGE_CONFIG.BACKDROP_SIZES.MEDIUM,
    large: TMDB_IMAGE_CONFIG.BACKDROP_SIZES.LARGE,
    original: TMDB_IMAGE_CONFIG.BACKDROP_SIZES.ORIGINAL,
  };
  
  return `${TMDB_IMAGE_CONFIG.BASE_URL}/${sizeMap[size]}${backdropPath}`;
}

export function getProfileUrl(
  profilePath: string | null,
  size: 'small' | 'medium' | 'large' = 'medium'
): string | null {
  if (!profilePath) {
    return null;
  }

  const sizeMap = {
    small: TMDB_IMAGE_CONFIG.PROFILE_SIZES.SMALL,
    medium: TMDB_IMAGE_CONFIG.PROFILE_SIZES.MEDIUM,
    large: TMDB_IMAGE_CONFIG.PROFILE_SIZES.LARGE,
  };

  return `${TMDB_IMAGE_CONFIG.BASE_URL}/${sizeMap[size]}${profilePath}`;
}

export function getYouTubeEmbedUrl(videoKey: string): string {
  return `https://www.youtube.com/embed/${videoKey}?autoplay=0&rel=0`;
}

export function findMainTrailer(videos: MovieVideo[]): MovieVideo | null {
  if (!videos || videos.length === 0) return null;

  const official = videos.find(v => v.official && v.type === 'Trailer' && v.site === 'YouTube');
  if (official) return official;

  const trailer = videos.find(v => v.type === 'Trailer' && v.site === 'YouTube');
  if (trailer) return trailer;

  const teaser = videos.find(v => v.type === 'Teaser' && v.site === 'YouTube');
  if (teaser) return teaser;

  return videos.find(v => v.site === 'YouTube') || null;
}
