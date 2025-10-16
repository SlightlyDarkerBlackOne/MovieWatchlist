/**
 * Tests for movie service
 */

import movieService from './movieService';
import api from './api';
import cacheService from '../utils/cacheService';
import { mockMovie, mockMovieDetails, mockMovieVideo, mockMovieCredits, mockMovies } from '../__tests__/fixtures/movieFixtures';
import { MovieSearchResult } from '../types/movie.types';

// Mock dependencies
jest.mock('./api');
jest.mock('../utils/cacheService');

const mockedApi = api as jest.Mocked<typeof api>;
const mockedCache = cacheService as jest.Mocked<typeof cacheService>;

describe('MovieService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedCache.get.mockReturnValue(null); // Default to cache miss
    mockedCache.getStats.mockReturnValue({ totalEntries: 0, totalSize: 0, version: '1.0' });
  });

  describe('searchMovies', () => {
    const mockSearchResult: MovieSearchResult = {
      movies: mockMovies,
      totalResults: 3,
      totalPages: 1,
      currentPage: 1,
    };

    it('should return cached results when available', async () => {
      mockedCache.get.mockReturnValue(mockSearchResult);

      const result = await movieService.searchMovies('fight club', 1);

      expect(result).toEqual(mockSearchResult);
      expect(mockedApi.get).not.toHaveBeenCalled();
      expect(mockedCache.get).toHaveBeenCalledWith(expect.stringContaining('fight club'));
    });

    it('should fetch from API on cache miss', async () => {
      mockedCache.get.mockReturnValue(null);
      mockedApi.get.mockResolvedValue({ data: mockMovies });

      const result = await movieService.searchMovies('fight club', 1);

      expect(result.movies).toEqual(mockMovies);
      expect(mockedApi.get).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should handle empty search results', async () => {
      mockedApi.get.mockResolvedValue({ data: [] });

      const result = await movieService.searchMovies('nonexistent movie', 1);

      expect(result.movies).toEqual([]);
      expect(result.totalResults).toBe(0);
    });

    it('should throw error on API failure', async () => {
      mockedApi.get.mockRejectedValue({
        response: { data: { message: 'API error' } }
      });

      await expect(movieService.searchMovies('test', 1)).rejects.toThrow();
    });
  });

  describe('getPopularMovies', () => {
    const mockPopularResult: MovieSearchResult = {
      movies: mockMovies,
      totalResults: 3,
      totalPages: 1,
      currentPage: 1,
    };

    it('should return cached popular movies', async () => {
      mockedCache.get.mockReturnValue(mockPopularResult);

      const result = await movieService.getPopularMovies(1);

      expect(result).toEqual(mockPopularResult);
      expect(mockedApi.get).not.toHaveBeenCalled();
    });

    it('should fetch popular movies from API on cache miss', async () => {
      mockedCache.get.mockReturnValue(null);
      mockedApi.get.mockResolvedValue({ data: mockMovies });

      const result = await movieService.getPopularMovies(1);

      expect(result.movies).toEqual(mockMovies);
      expect(mockedApi.get).toHaveBeenCalled();
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should handle different pages', async () => {
      mockedApi.get.mockResolvedValue({ data: mockMovies });

      await movieService.getPopularMovies(2);

      expect(mockedApi.get).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({ params: { page: 2 } })
      );
    });
  });

  describe('getMovieDetailsByTmdbId', () => {
    const mockCompleteData = {
      movie: mockMovieDetails,
      credits: mockMovieCredits,
      videos: [mockMovieVideo],
    };

    it('should return cached movie details', async () => {
      mockedCache.get.mockReturnValue(mockCompleteData);

      const result = await movieService.getMovieDetailsByTmdbId(550);

      expect(result).toEqual(mockCompleteData);
      expect(mockedApi.get).not.toHaveBeenCalled();
    });

    it('should fetch movie details from API on cache miss', async () => {
      mockedCache.get.mockReturnValue(null);
      mockedApi.get.mockResolvedValue({
        data: {
          movie: mockMovieDetails,
          credits: { cast: [], crew: [] },
          videos: []
        }
      });

      const result = await movieService.getMovieDetailsByTmdbId(550);

      expect(result.movie).toBeDefined();
      expect(mockedApi.get).toHaveBeenCalledWith('/Movies/tmdb/550');
      expect(mockedCache.set).toHaveBeenCalled();
    });

    it('should throw error for invalid movie ID', async () => {
      mockedApi.get.mockRejectedValue({
        response: { data: { message: 'Movie not found' } }
      });

      await expect(movieService.getMovieDetailsByTmdbId(999999)).rejects.toThrow();
    });
  });

  describe('URL generators', () => {
    describe('getPosterUrl', () => {
      it('should generate correct poster URL for valid path', () => {
        const url = movieService.getPosterUrl('/test.jpg', 'medium');
        expect(url).toContain('https://image.tmdb.org/t/p/');
        expect(url).toContain('/test.jpg');
      });

      it('should return placeholder for null poster path', () => {
        const url = movieService.getPosterUrl(null, 'medium');
        expect(url).toBe('/placeholder-movie.png');
      });

      it('should handle different sizes', () => {
        const smallUrl = movieService.getPosterUrl('/test.jpg', 'small');
        const largeUrl = movieService.getPosterUrl('/test.jpg', 'large');
        
        expect(smallUrl).toBeDefined();
        expect(largeUrl).toBeDefined();
        expect(smallUrl).not.toEqual(largeUrl);
      });
    });

    describe('getBackdropUrl', () => {
      it('should generate correct backdrop URL', () => {
        const url = movieService.getBackdropUrl('/backdrop.jpg', 'original');
        expect(url).toContain('https://image.tmdb.org/t/p/');
        expect(url).toContain('/backdrop.jpg');
      });

      it('should return null for null backdrop path', () => {
        const url = movieService.getBackdropUrl(null, 'original');
        expect(url).toBeNull();
      });
    });

    describe('getProfileUrl', () => {
      it('should generate correct profile URL', () => {
        const url = movieService.getProfileUrl('/profile.jpg', 'small');
        expect(url).toContain('https://image.tmdb.org/t/p/');
        expect(url).toContain('/profile.jpg');
      });

      it('should return null for null profile path', () => {
        const url = movieService.getProfileUrl(null, 'small');
        expect(url).toBeNull();
      });
    });
  });

  describe('findMainTrailer', () => {
    it('should find official YouTube trailer', () => {
      const videos = [
        { ...mockMovieVideo, type: 'Teaser', official: false },
        { ...mockMovieVideo, type: 'Trailer', official: true },
        { ...mockMovieVideo, type: 'Clip', official: false },
      ];

      const trailer = movieService.findMainTrailer(videos);

      expect(trailer).toBeDefined();
      expect(trailer?.type).toBe('Trailer');
      expect(trailer?.official).toBe(true);
    });

    it('should return first trailer if no official trailer', () => {
      const videos = [
        { ...mockMovieVideo, type: 'Trailer', official: false },
      ];

      const trailer = movieService.findMainTrailer(videos);

      expect(trailer).toBeDefined();
      expect(trailer?.type).toBe('Trailer');
    });

    it('should return null for empty video list', () => {
      const trailer = movieService.findMainTrailer([]);
      expect(trailer).toBeNull();
    });

    it('should return null if no trailers exist', () => {
      const videos = [
        { ...mockMovieVideo, type: 'Clip', official: false, site: 'Vimeo' },
        { ...mockMovieVideo, type: 'Featurette', official: false, site: 'Vimeo' },
      ];

      const trailer = movieService.findMainTrailer(videos);
      expect(trailer).toBeNull();
    });
  });

  describe('clearPopularMoviesCache', () => {
    it('should clear cache for all popular movie pages', () => {
      movieService.clearPopularMoviesCache();

      // Should call clear for multiple pages
      expect(mockedCache.clear).toHaveBeenCalled();
      expect(mockedCache.clear).toHaveBeenCalledTimes(10); // Clears pages 1-10
    });
  });
});


