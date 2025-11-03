/**
 * Tests for movie service utilities
 * 
 * NOTE: Network functions (searchMovies, getPopularMovies, getMovieDetailsByTmdbId)
 * have been moved to RTK Query in store/api/moviesApi.ts
 * See __tests__/integration/moviesApi.test.ts for integration tests
 */

import * as movieService from './movieService';
import { mockMovieVideo } from '../__tests__/fixtures/movieFixtures';

describe('MovieService Utilities', () => {

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

  describe('getYouTubeEmbedUrl', () => {
    it('should generate correct YouTube embed URL', () => {
      const url = movieService.getYouTubeEmbedUrl('dQw4w9WgXcQ');
      
      expect(url).toBe('https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=0&rel=0');
    });

    it('should handle different video keys', () => {
      const url1 = movieService.getYouTubeEmbedUrl('abc123');
      const url2 = movieService.getYouTubeEmbedUrl('xyz789');
      
      expect(url1).toContain('abc123');
      expect(url2).toContain('xyz789');
      expect(url1).not.toEqual(url2);
    });
  });
});


