import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { 
  getUserWatchlist,
  addToWatchlist,
  updateWatchlistItem,
  removeFromWatchlist,
  getWatchlistStatistics
} from '../../services/watchlistService';
import { WatchlistItem, WatchlistStatistics, AddToWatchlistRequest, UpdateWatchlistRequest } from '../../types/watchlist.types';

const baseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5250/api';

export const watchlistApi = createApi({
  reducerPath: 'watchlistApi',
  baseQuery: fetchBaseQuery({ baseUrl }),
  tagTypes: ['Watchlist', 'WatchlistStats'],
  endpoints: (builder) => ({
    getWatchlist: builder.query<WatchlistItem[], number>({
      async queryFn(userId) {
        try {
          const result = await getUserWatchlist(userId);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      providesTags: ['Watchlist'],
    }),
    addToWatchlist: builder.mutation<WatchlistItem, { userId: number; request: AddToWatchlistRequest }>({
      async queryFn({ userId, request }) {
        try {
          const result = await addToWatchlist(userId, request);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    updateWatchlistItem: builder.mutation<WatchlistItem, { userId: number; itemId: number; request: UpdateWatchlistRequest }>({
      async queryFn({ userId, itemId, request }) {
        try {
          const result = await updateWatchlistItem(userId, itemId, request);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    removeFromWatchlist: builder.mutation<void, { userId: number; itemId: number }>({
      async queryFn({ userId, itemId }) {
        try {
          await removeFromWatchlist(userId, itemId);
          return { data: undefined };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      invalidatesTags: ['Watchlist', 'WatchlistStats'],
    }),
    getWatchlistStatistics: builder.query<WatchlistStatistics, number>({
      async queryFn(userId) {
        try {
          const result = await getWatchlistStatistics(userId);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
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
