import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { WatchlistItem, WatchlistStatistics, AddToWatchlistRequest, UpdateWatchlistRequest } from '../../types/watchlist.types';
import { STORAGE_KEYS } from '../../utils/constants';

const baseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5250/api';

export const watchlistApi = createApi({
  reducerPath: 'watchlistApi',
  baseQuery: fetchBaseQuery({ 
    baseUrl,
    prepareHeaders: (headers) => {
      const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['Watchlist', 'WatchlistStats'],
  endpoints: (builder) => ({
    getWatchlist: builder.query<WatchlistItem[], number>({
      query: (userId) => `/Watchlist/user/${userId}`,
      providesTags: ['Watchlist'],
    }),
    addToWatchlist: builder.mutation<WatchlistItem, { userId: number; request: AddToWatchlistRequest }>({
      query: ({ userId, request }) => ({
        url: `/Watchlist/user/${userId}/add`,
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    updateWatchlistItem: builder.mutation<WatchlistItem, { userId: number; itemId: number; request: UpdateWatchlistRequest }>({
      query: ({ userId, itemId, request }) => ({
        url: `/Watchlist/user/${userId}/item/${itemId}`,
        method: 'PUT',
        body: request,
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    removeFromWatchlist: builder.mutation<void, { userId: number; itemId: number }>({
      query: ({ userId, itemId }) => ({
        url: `/Watchlist/user/${userId}/item/${itemId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    getWatchlistStatistics: builder.query<WatchlistStatistics, number>({
      query: (userId) => `/Watchlist/user/${userId}/statistics`,
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
