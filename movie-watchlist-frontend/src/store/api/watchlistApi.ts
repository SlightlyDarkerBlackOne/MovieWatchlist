import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { WatchlistItem, WatchlistStatistics, AddToWatchlistRequest, UpdateWatchlistRequest } from '../../types/watchlist.types';
 

const baseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5250/api';

export const watchlistApi = createApi({
  reducerPath: 'watchlistApi',
  baseQuery: fetchBaseQuery({ baseUrl, credentials: 'include' }),
  tagTypes: ['Watchlist', 'WatchlistStats'],
  endpoints: (builder) => ({
    getWatchlist: builder.query<WatchlistItem[], void>({
      query: () => `/Watchlist/me/watchlist`,
      providesTags: ['Watchlist'],
    }),
    addToWatchlist: builder.mutation<WatchlistItem, AddToWatchlistRequest>({
      query: (request) => ({
        url: `/Watchlist/me/watchlist/add`,
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    updateWatchlistItem: builder.mutation<WatchlistItem, { itemId: number; request: UpdateWatchlistRequest }>({
      query: ({ itemId, request }) => ({
        url: `/Watchlist/me/watchlist/item/${itemId}`,
        method: 'PUT',
        body: request,
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    removeFromWatchlist: builder.mutation<void, number>({
      query: (itemId) => ({
        url: `/Watchlist/me/watchlist/item/${itemId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    getWatchlistStatistics: builder.query<WatchlistStatistics, void>({
      query: () => `/Watchlist/me/watchlist/statistics`,
      providesTags: ['WatchlistStats'],
    }),
  }),
});

export const {
  useGetWatchlistQuery,
  useAddToWatchlistMutation,
  useUpdateWatchlistItemMutation,
  useRemoveFromWatchlistMutation,
  useGetWatchlistStatisticsQuery
} = watchlistApi;
