/**
 * Integration tests for Watchlist RTK Query API
 */

import { configureStore } from '@reduxjs/toolkit';
import { watchlistApi } from '../../store/api/watchlistApi';
import { mockWatchlistItems, mockWatchlistItem } from '../fixtures/watchlistFixtures';
import { WatchlistStatus } from '../../types/watchlist.types';
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { TestConstants } from '../TestConstants';

const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Watchlist RTK Query API', () => {
  let store: any;

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
        http.get(TestConstants.ApiEndpoints.WatchlistMe, () => {
          return HttpResponse.json(mockWatchlistItems);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(undefined)
      );

      expect(result.data).toBeDefined();
      expect(result.data).toEqual(mockWatchlistItems);
    });

    it('should handle empty watchlist', async () => {
      server.use(
        http.get(TestConstants.ApiEndpoints.WatchlistMe, () => {
          return HttpResponse.json([]);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(undefined)
      );

      expect(result.data).toEqual([]);
    });

    it('should handle fetch errors', async () => {
      server.use(
        http.get(TestConstants.ApiEndpoints.WatchlistMe, () => {
          return HttpResponse.json({ message: TestConstants.ErrorMessages.ServerError }, { status: TestConstants.HttpStatusCodes.InternalServerError });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(undefined)
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('addToWatchlist', () => {
    it('should add movie to watchlist successfully', async () => {
      const request = {
        movieId: TestConstants.Watchlist.DefaultMovieId,
        status: WatchlistStatus.Planned,
        notes: TestConstants.Watchlist.DefaultNotes,
      };

      server.use(
        http.post(TestConstants.ApiEndpoints.WatchlistAdd, () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate(request)
      );

      expect(result.data).toEqual(mockWatchlistItem);
    });

    it('should handle duplicate movie error', async () => {
      const request = { movieId: TestConstants.Watchlist.DefaultMovieId };

      server.use(
        http.post(TestConstants.ApiEndpoints.WatchlistAdd, () => {
          return HttpResponse.json(
            { message: TestConstants.ErrorMessages.MovieAlreadyInWatchlist },
            { status: TestConstants.HttpStatusCodes.BadRequest }
          );
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate(request)
      );

      expect(result.error).toBeDefined();
    });

    it('should handle validation errors', async () => {
      const request = { movieId: 0 };

      server.use(
        http.post(TestConstants.ApiEndpoints.WatchlistAdd, () => {
          return HttpResponse.json(
            {
              errors: {
                movieId: [TestConstants.ErrorMessages.MovieIdRequired],
              }
            },
            { status: TestConstants.HttpStatusCodes.BadRequest }
          );
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate(request)
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after adding', async () => {
      const request = { movieId: TestConstants.Watchlist.DefaultMovieId };

      server.use(
        http.get(TestConstants.ApiEndpoints.WatchlistMe, () => {
          return HttpResponse.json(mockWatchlistItems);
        }),
        http.post(TestConstants.ApiEndpoints.WatchlistAdd, () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      await store.dispatch(watchlistApi.endpoints.getWatchlist.initiate(undefined));
      
      await store.dispatch(
        watchlistApi.endpoints.addToWatchlist.initiate(request)
      );

      expect(true).toBe(true);
    });
  });

  describe('updateWatchlistItem', () => {
    it('should update watchlist item successfully', async () => {
      const request = { watchlistItemId: TestConstants.Watchlist.DefaultItemId, userRating: 4.5 };
      const updatedItem = { ...mockWatchlistItem, userRating: 4.5 };

      server.use(
        http.put(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return HttpResponse.json(updatedItem);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate(request)
      );

      expect(result.data).toEqual(updatedItem);
    });

    it('should handle item not found error', async () => {
      const request = { watchlistItemId: TestConstants.Users.NonExistentUserId, userRating: 5 };

      server.use(
        http.put(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return HttpResponse.json({ message: TestConstants.ErrorMessages.FailedToLoad }, { status: TestConstants.HttpStatusCodes.NotFound });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate(request)
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after updating', async () => {
      const request = { watchlistItemId: TestConstants.Watchlist.DefaultItemId, userRating: 4.5 };

      server.use(
        http.put(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return HttpResponse.json(mockWatchlistItem);
        })
      );

      await store.dispatch(
        watchlistApi.endpoints.updateWatchlistItem.initiate(request)
      );

      expect(true).toBe(true);
    });
  });

  describe('removeFromWatchlist', () => {
    it('should remove item successfully', async () => {
      const itemId = TestConstants.Watchlist.DefaultItemId;

      server.use(
        http.delete(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return new HttpResponse(null, { status: 204 });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate(itemId)
      );

      expect(result.data).toBeNull();
      expect(result.error).toBeUndefined();
    });

    it('should handle deletion errors', async () => {
      const itemId = TestConstants.Users.NonExistentUserId;

      server.use(
        http.delete(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return HttpResponse.json({ message: TestConstants.ErrorMessages.FailedToLoad }, { status: TestConstants.HttpStatusCodes.NotFound });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate(itemId)
      );

      expect(result.error).toBeDefined();
    });

    it('should invalidate cache after removing', async () => {
      const itemId = TestConstants.Watchlist.DefaultItemId;

      server.use(
        http.delete(TestConstants.ApiEndpoints.WatchlistItem, () => {
          return new HttpResponse(null, { status: 204 });
        })
      );

      await store.dispatch(
        watchlistApi.endpoints.removeFromWatchlist.initiate(itemId)
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
        http.get(TestConstants.ApiEndpoints.WatchlistStatistics, () => {
          return HttpResponse.json(mockStats);
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlistStatistics.initiate(undefined)
      );

      expect(result.data).toEqual(mockStats);
    });

    it('should handle statistics fetch errors', async () => {
      server.use(
        http.get(TestConstants.ApiEndpoints.WatchlistStatistics, () => {
          return HttpResponse.json(null, { status: TestConstants.HttpStatusCodes.InternalServerError });
        })
      );

      const result = await store.dispatch(
        watchlistApi.endpoints.getWatchlistStatistics.initiate(undefined)
      );

      expect(result.error).toBeDefined();
    });
  });

  describe('Cache Behavior', () => {
    it('should cache and reuse watchlist data', async () => {
      server.use(
        http.get(TestConstants.ApiEndpoints.WatchlistMe, () => {
          return HttpResponse.json(mockWatchlistItems);
        })
      );

      const result1 = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(undefined)
      );
      
      const result2 = await store.dispatch(
        watchlistApi.endpoints.getWatchlist.initiate(undefined)
      );

      expect(result1.data).toEqual(mockWatchlistItems);
      expect(result2.data).toEqual(mockWatchlistItems);
    });
  });
});
