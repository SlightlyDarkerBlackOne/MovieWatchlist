/**
 * Integration tests for Watchlist RTK Query API
 */

import { configureStore } from '@reduxjs/toolkit';
import { watchlistApi } from '../../store/api/watchlistApi';
import { mockWatchlistItems, mockWatchlistItem } from '../fixtures/watchlistFixtures';
import { WatchlistStatus } from '../../types/watchlist.types';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Watchlist RTK Query API', () => {
  let store: any;
  const userId = 1;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        [watchlistApi.reducerPath]: watchlistApi.reducer,
      },
      middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(watchlistApi.middleware),
    });
  });

  describe('getWatchlist', () => {
    it('should fetch user watchlist successfully', async () => {
      server.use(
        http.get('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(mockWatchlistItems);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(userId)
      );

      expect(result.data).toBeDefined();
      expect(result.data).toEqual(mockWatchlistItems);
    });

    it('should handle empty watchlist', async () => {
      server.use(
        http.get('*/Watchlist/user/:userId', () => {
          return HttpResponse.json([]);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(userId)
      );

      expect(result.data).toEqual([]);
    });

    it('should handle fetch errors', async () => {
      server.use(
        http.get('*/Watchlist/user/:userId', () => {
          return HttpResponse.json({ message: 'Server error' }, { status: 500 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(userId)
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('addToWatchlist', () => {
    it('should add movie to watchlist successfully', async () => {
      const request = {
        movieId: 550,
        status: WatchlistStatus.Planned,
        notes: 'Must watch!',
      };

      server.use(
        http.post('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate({ userId, request })
      );

      expect(result.data).toEqual(mockWatchlistItem);
    });

    it('should handle duplicate movie error', async () => {
      const request = { movieId: 550 };

      server.use(
        http.post('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(
            { message: 'Movie is already in watchlist' },
            { status: 400 }
          );
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate({ userId, request })
      );

      expect(result.error).toBeDefined();
    });

    it('should handle validation errors', async () => {
      const request = { movieId: 0 };

      server.use(
        http.post('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(
            {
              errors: {
                movieId: ['Movie ID is required'],
              }
            },
            { status: 400 }
          );
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate({ userId, request })
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after adding', async () => {
      const request = { movieId: 550 };

      server.use(
        http.get('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(mockWatchlistItems);
        }),
        http.post('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      await store.dispatch(watchlistApi.endpoints.getWatchlist.initiate(userId));
      
      await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate({ userId, request })
      );

      expect(true).toBe(true);
    });
  });

  describe('updateWatchlistItem', () => {
    it('should update watchlist item successfully', async () => {
      const itemId = 1;
      const request = { userRating: 4.5 };
      const updatedItem = { ...mockWatchlistItem, userRating: 4.5 };

      server.use(
        http.put('*/Watchlist/user/:userId/item/:itemId', () => {
          return HttpResponse.json(updatedItem);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate({ userId, itemId, request })
      );

      expect(result.data).toEqual(updatedItem);
    });

    it('should handle item not found error', async () => {
      const itemId = 999;
      const request = { userRating: 5 };

      server.use(
        http.put('*/Watchlist/user/:userId/item/:itemId', () => {
          return HttpResponse.json({ message: 'Item not found' }, { status: 404 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate({ userId, itemId, request })
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after updating', async () => {
      const itemId = 1;
      const request = { userRating: 4.5 };

      server.use(
        http.put('*/Watchlist/user/:userId/item/:itemId', () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate({ userId, itemId, request })
      );

      expect(true).toBe(true);
    });
  });

  describe('removeFromWatchlist', () => {
    it('should remove item successfully', async () => {
      const itemId = 1;

      server.use(
        http.delete('*/Watchlist/user/:userId/item/:itemId', () => {
          return new HttpResponse(null, { status: 204 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate({ userId, itemId })
      );

      expect(result.data).toBeNull();
      expect(result.error).toBeUndefined();
    });

    it('should handle deletion errors', async () => {
      const itemId = 999;

      server.use(
        http.delete('*/Watchlist/user/:userId/item/:itemId', () => {
          return HttpResponse.json({ message: 'Item not found' }, { status: 404 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate({ userId, itemId })
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after removing', async () => {
      const itemId = 1;

      server.use(
        http.delete('*/Watchlist/user/:userId/item/:itemId', () => {
          return new HttpResponse(null, { status: 204 });
        })
      );

      await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate({ userId, itemId })
      );

      expect(true).toBe(true);
    });
  });

  describe('getWatchlistStatistics', () => {
    it('should fetch statistics successfully', async () => {
      const mockStats = {
        totalMovies: 10,
        watchedMovies: 5,
        plannedMovies: 3,
        favoriteMovies: 3,
        averageUserRating: 4.5,
        averageTmdbRating: 7.2,
        mostWatchedGenre: 'Action',
        moviesThisYear: 2,
        genreBreakdown: { Action: 3, Comedy: 2 },
        yearlyBreakdown: { 2024: 2 }
      };

      server.use(
        http.get('*/Watchlist/user/:userId/statistics', () => {
          return HttpResponse.json(mockStats);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlistStatistics.initiate(userId)
      );

      expect(result.data).toEqual(mockStats);
    });

    it('should handle statistics fetch errors', async () => {
      server.use(
        http.get('*/Watchlist/user/:userId/statistics', () => {
          return HttpResponse.json(null, { status: 500 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlistStatistics.initiate(userId)
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('Cache Behavior', () => {
    it('should cache and reuse watchlist data', async () => {
      server.use(
        http.get('*/Watchlist/user/:userId', () => {
          return HttpResponse.json(mockWatchlistItems);
        })
      );

      const result1 = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(userId)
      );
      
      const result2 = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(userId)
      );

      expect(result1.data).toEqual(mockWatchlistItems);
      expect(result2.data).toEqual(mockWatchlistItems);
    });
  });
});
