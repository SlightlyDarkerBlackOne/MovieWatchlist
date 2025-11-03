import { MovieVideo } from '../types/movie.types';

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
