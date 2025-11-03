/**
 * Integration tests for Movies RTK Query API
 */

import { configureStore } from '@reduxjs/toolkit';
import { moviesApi } from '../../store/api/moviesApi';
import { mockMovies, mockMovieDetails } from '../fixtures/movieFixtures';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Movies RTK Query API', () => {
  let store: any;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        [moviesApi.reducerPath]: moviesApi.reducer,
      },
      middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(moviesApi.middleware),
    });
  });

  describe('searchMovies', () => {
    it('should search movies successfully', async () => {
      server.use(
        http.get('*/Movies/search', () => {
          return HttpResponse.json(mockMovies);
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.searchMovies.initiate({ query: 'fight club', page: 1 })
      );

      expect(result.data).toBeDefined();
      expect(result.data?.movies).toEqual(mockMovies);
      expect(result.data?.totalResults).toBe(mockMovies.length);
    });

    it('should handle search errors', async () => {
      server.use(
        http.get('*/Movies/search', () => {
          return HttpResponse.json({ message: 'Server error' }, { status: 500 });
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.searchMovies.initiate({ query: 'test', page: 1 })
      );

      expect(result.error).toBeDefined();
    });

    it('should pass correct query parameters', async () => {
      let capturedQuery = '';
      let capturedPage = '';

      server.use(
        http.get('*/Movies/search', ({ request }) => {
          const url = new URL(request.url);
          capturedQuery = url.searchParams.get('query') || '';
          capturedPage = url.searchParams.get('page') || '';
          return HttpResponse.json([]);
        })
      );

      await store.dispatch(
        moviesApi.endpoints.searchMovies.initiate({ query: 'inception', page: 2 })
      );

      expect(capturedQuery).toBe('inception');
      expect(capturedPage).toBe('2');
    });
  });

  describe('getPopularMovies', () => {
    it('should fetch popular movies successfully', async () => {
      server.use(
        http.get('*/Movies/popular', () => {
          return HttpResponse.json(mockMovies);
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getPopularMovies.initiate({ page: 1 })
      );

      expect(result.data).toBeDefined();
      expect(result.data?.movies).toEqual(mockMovies);
    });

    it('should handle different pages', async () => {
      let capturedPage = '';

      server.use(
        http.get('*/Movies/popular', ({ request }) => {
          const url = new URL(request.url);
          capturedPage = url.searchParams.get('page') || '';
          return HttpResponse.json([]);
        })
      );

      await store.dispatch(
        moviesApi.endpoints.getPopularMovies.initiate({ page: 3 })
      );

      expect(capturedPage).toBe('3');
    });
  });

  describe('getMovieDetails', () => {
    it('should fetch movie details successfully', async () => {
      const mockResponse = {
        movie: mockMovieDetails,
        credits: { cast: [], crew: [] },
        videos: []
      };

      server.use(
        http.get('*/Movies/tmdb/:tmdbId', () => {
          return HttpResponse.json(mockResponse);
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getMovieDetails.initiate({ tmdbId: 550 })
      );

      expect(result.data).toBeDefined();
      expect(result.data?.movie).toEqual(mockMovieDetails);
    });

    it('should transform credits data correctly', async () => {
      const mockResponse = {
        movie: mockMovieDetails,
        credits: {
          cast: [{ id: 1, name: 'Actor 1', character: 'Character 1' }],
          crew: [{ id: 2, name: 'Director 1', job: 'Director' }]
        },
        videos: []
      };

      server.use(
        http.get('*/Movies/tmdb/:tmdbId', () => {
          return HttpResponse.json(mockResponse);
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getMovieDetails.initiate({ tmdbId: 550 })
      );

      expect(result.data?.credits).toBeDefined();
      expect(result.data?.credits.cast).toHaveLength(1);
      expect(result.data?.credits.crew).toHaveLength(1);
    });

    it('should handle movie not found error', async () => {
      server.use(
        http.get('*/Movies/tmdb/:tmdbId', () => {
          return HttpResponse.json({ message: 'Movie not found' }, { status: 404 });
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getMovieDetails.initiate({ tmdbId: 999999 })
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('getPopularMoviesInfinite', () => {
    it('should fetch multiple pages of popular movies', async () => {
      server.use(
        http.get('*/Movies/popular', () => {
          return HttpResponse.json(mockMovies);
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getPopularMoviesInfinite.initiate({ limit: 3 })
      );

      expect(result.data).toBeDefined();
      expect(result.data).toHaveLength(3);
    });

    it('should handle errors during infinite loading', async () => {
      server.use(
        http.get('*/Movies/popular', () => {
          return HttpResponse.json(null, { status: 500 });
        })
      );

      const result = await store.dispatch(
        moviesApi.endpoints.getPopularMoviesInfinite.initiate({ limit: 2 })
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('Cache Invalidation', () => {
    it('should tag movies with Movies tag', async () => {
      server.use(
        http.get('*/Movies/search', () => {
          return HttpResponse.json(mockMovies);
        })
      );

      await store.dispatch(
        moviesApi.endpoints.searchMovies.initiate({ query: 'test', page: 1 })
      );

      const state: any = store.getState();
      const cacheKeys = Object.keys(state[moviesApi.reducerPath].queries);
      
      expect(cacheKeys.length).toBeGreaterThan(0);
    });

    it('should tag movie details with specific ID', async () => {
      const mockResponse = {
        movie: mockMovieDetails,
        credits: { cast: [], crew: [] },
        videos: []
      };

      server.use(
        http.get('*/Movies/tmdb/:tmdbId', () => {
          return HttpResponse.json(mockResponse);
        })
      );

      await store.dispatch(
        moviesApi.endpoints.getMovieDetails.initiate({ tmdbId: 550 })
      );

      const state: any = store.getState();
      const cacheKeys = Object.keys(state[moviesApi.reducerPath].queries);
      
      expect(cacheKeys.some(key => key.includes('550'))).toBe(true);
    });
  });
});
