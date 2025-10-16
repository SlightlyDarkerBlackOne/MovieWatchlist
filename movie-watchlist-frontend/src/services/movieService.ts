import api from './api';
import { API_ENDPOINTS, CACHE_CONFIG, CACHE_KEYS } from '../utils/constants';
import { Movie, MovieSearchParams, MovieSearchResult, MovieDetails, MovieVideo, MovieCredits, CastMember } from '../types/movie.types';
import cacheService from '../utils/cacheService';
import { transformCastMember, transformCrewMember, transformVideo } from '../utils/tmdbTransformers';
import { AxiosError } from '../types/error.types';

/**
 * TMDB Image Configuration
 * See: https://developers.themoviedb.org/3/getting-started/images
 */
const TMDB_IMAGE_CONFIG = {
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

class MovieService {
  /**
   * Search for movies by title
   */
  async searchMovies(query: string, page: number = 1): Promise<MovieSearchResult> {
    try {
      // Create cache key
      const cacheKey = `${CACHE_KEYS.SEARCH_RESULTS}_${query.toLowerCase()}_page_${page}`;
      
      // Check cache first
      const cached = cacheService.get<MovieSearchResult>(cacheKey);
      if (cached) {
        console.log(`Search results for "${query}" loaded from cache`);
        return cached;
      }

      // Cache miss - fetch from API
      console.log(`Searching for "${query}" via API`);
      const response = await api.get(API_ENDPOINTS.MOVIES.SEARCH, {
        params: { query, page }
      });
      
      // Backend returns array of movies, wrap in MovieSearchResult
      const movies = Array.isArray(response.data) ? response.data : [];
      const result = {
        movies,
        totalResults: movies.length,
        totalPages: 1,
        currentPage: page
      };

      // Cache the results (30 minutes)
      const ttlHours = CACHE_CONFIG.SEARCH_CACHE_TTL_MINUTES / 60;
      cacheService.set(cacheKey, result, ttlHours);

      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Movie search error:', axiosError);
      throw new Error(axiosError.response?.data?.message || 'Failed to search movies');
    }
  }

  /**
   * Get popular movies (with caching)
   */
  async getPopularMovies(page: number = 1): Promise<MovieSearchResult> {
    try {
      // Try cache first
      const cacheKey = `${CACHE_KEYS.POPULAR_MOVIES_PREFIX}${page}`;
      const cached = cacheService.get<MovieSearchResult>(cacheKey);
      
      if (cached) {
        console.log(`Popular movies page ${page} loaded from cache`);
        return cached;
      }
      
      // Cache miss - fetch from API
      console.log(`Popular movies page ${page} fetching from API`);
      const response = await api.get(API_ENDPOINTS.MOVIES.POPULAR, {
        params: { page }
      });
      
      // Backend returns array of movies, wrap in MovieSearchResult
      const movies = Array.isArray(response.data) ? response.data : [];
      const result: MovieSearchResult = {
        movies,
        totalResults: movies.length,
        totalPages: 1,
        currentPage: page
      };
      
      // Store in cache for 3 minutes to get fresh variety
      const ttlHours = CACHE_CONFIG.POPULAR_MOVIES_CACHE_MINUTES / 60;
      cacheService.set(cacheKey, result, ttlHours);
      
      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get popular movies error:', axiosError);
      throw new Error(axiosError.response?.data?.message || 'Failed to get popular movies');
    }
  }

  /**
   * Get movie details by ID
   */
  async getMovieDetails(movieId: number): Promise<Movie> {
    try {
      const response = await api.get(API_ENDPOINTS.MOVIES.DETAILS(movieId));
      return response.data;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get movie details error:', axiosError);
      throw new Error(axiosError.response?.data?.message || 'Failed to get movie details');
    }
  }

  /**
   * Get movies by genre
   */
  async getMoviesByGenre(genre: string, page: number = 1): Promise<MovieSearchResult> {
    try {
      const response = await api.get(API_ENDPOINTS.MOVIES.BY_GENRE(genre), {
        params: { page }
      });
      
      // Backend returns array of movies, wrap in MovieSearchResult
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

  /**
   * Get poster URL for a movie
   * @param posterPath - The poster path from TMDB (e.g., "/abc123.jpg")
   * @param size - The desired image size
   * @returns Full URL to the poster image, or null if no path provided
   */
  getPosterUrl(
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

  /**
   * Get backdrop URL for a movie
   * @param backdropPath - The backdrop path from TMDB (e.g., "/xyz789.jpg")
   * @param size - The desired image size
   * @returns Full URL to the backdrop image, or null if no path provided
   */
  getBackdropUrl(
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

  /**
   * Get profile image URL for cast/crew
   * @param profilePath - The profile path from TMDB
   * @param size - The desired image size
   * @returns Full URL to the profile image
   */
  getProfileUrl(
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

  /**
   * Get complete movie data (details + credits + videos) in one API call
   * This uses cached data from the database to minimize TMDB API calls
   * Also implements frontend caching to reduce backend API calls on page refreshes
   */
  async getMovieDetailsByTmdbId(tmdbId: number): Promise<{
    movie: MovieDetails;
    credits: MovieCredits;
    videos: MovieVideo[];
  }> {
    try {
      const cacheKey = `${CACHE_KEYS.MOVIE_DETAILS}_${tmdbId}`;
      
      // Check frontend cache first
      const cachedData = cacheService.get<{
        movie: MovieDetails;
        credits: MovieCredits;
        videos: MovieVideo[];
      }>(cacheKey);
      
      if (cachedData) {
        console.log(`Using cached movie details for TMDB ID: ${tmdbId}`);
        return cachedData;
      }

      // Fetch from backend (which checks its own database cache)
      const response = await api.get(`/Movies/tmdb/${tmdbId}`);
      const data = response.data;
      
      // Transform credits data from snake_case to camelCase
      const transformedCredits = {
        cast: (data.credits?.cast || []).map(transformCastMember),
        crew: (data.credits?.crew || []).map(transformCrewMember)
      };
      
      // Transform videos data from snake_case to camelCase
      const transformedVideos = (data.videos || []).map(transformVideo);
      
      const result = {
        movie: data.movie,
        credits: transformedCredits,
        videos: transformedVideos
      };

      // Cache in frontend for 1 hour
      cacheService.set(cacheKey, result, 1); // 1 hour TTL
      
      return result;
    } catch (error) {
      const axiosError = error as AxiosError;
      console.error('Get movie details error:', axiosError);
      throw new Error(axiosError.response?.data?.message || 'Failed to get movie details');
    }
  }

  /**
   * Get YouTube embed URL from video key
   */
  getYouTubeEmbedUrl(videoKey: string): string {
    return `https://www.youtube.com/embed/${videoKey}?autoplay=0&rel=0`;
  }

  /**
   * Find the main trailer from videos array
   */
  findMainTrailer(videos: MovieVideo[]): MovieVideo | null {
    if (!videos || videos.length === 0) return null;

    // Priority: Official trailer > Trailer > Teaser
    const official = videos.find(v => v.official && v.type === 'Trailer' && v.site === 'YouTube');
    if (official) return official;

    const trailer = videos.find(v => v.type === 'Trailer' && v.site === 'YouTube');
    if (trailer) return trailer;

    const teaser = videos.find(v => v.type === 'Teaser' && v.site === 'YouTube');
    if (teaser) return teaser;

    // Return first YouTube video
    return videos.find(v => v.site === 'YouTube') || null;
  }

  /**
   * Clear cached popular movies
   */
  clearPopularMoviesCache(): void {
    // Clear all pages of popular movies
    const stats = cacheService.getStats();
    console.log(`Clearing popular movies cache (${stats.totalEntries} entries)`);
    
    // Clear cache keys that match the popular movies pattern
    for (let page = 1; page <= 10; page++) {
      const cacheKey = `${CACHE_KEYS.POPULAR_MOVIES_PREFIX}${page}`;
      cacheService.clear(cacheKey);
    }
    
    console.log('Popular movies cache cleared');
  }
}

export default new MovieService();

