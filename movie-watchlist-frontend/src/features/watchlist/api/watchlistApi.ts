import { WatchlistItem, WatchlistStatistics, AddToWatchlistRequest, UpdateWatchlistRequest } from '../model/watchlist.types';
import { API_ENDPOINTS, HTTP_METHODS, RTK_TAG_TYPES } from '../../../shared/constants/appConstants';
import { baseApiSlice } from '../../../shared/api/baseApiSlice';

export const watchlistApi = baseApiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWatchlist: builder.query<WatchlistItem[], void>({
      query: () => API_ENDPOINTS.WATCHLIST.ME,
      providesTags: [RTK_TAG_TYPES.WATCHLIST],
    }),
    addToWatchlist: builder.mutation<WatchlistItem, AddToWatchlistRequest>({
      query: (request) => ({
        url: API_ENDPOINTS.WATCHLIST.ADD,
        method: HTTP_METHODS.POST,
        body: request,
      }),
      invalidatesTags: [RTK_TAG_TYPES.WATCHLIST, RTK_TAG_TYPES.WATCHLIST_STATS],
    }),
    updateWatchlistItem: builder.mutation<WatchlistItem, UpdateWatchlistRequest>({
      query: (request) => ({
        url: API_ENDPOINTS.WATCHLIST.UPDATE_ITEM,
        method: HTTP_METHODS.PUT,
        body: request,
      }),
      invalidatesTags: [RTK_TAG_TYPES.WATCHLIST, RTK_TAG_TYPES.WATCHLIST_STATS],
    }),
    removeFromWatchlist: builder.mutation<void, number>({
      query: (itemId) => ({
        url: API_ENDPOINTS.WATCHLIST.ITEM(itemId),
        method: HTTP_METHODS.DELETE,
      }),
      invalidatesTags: [RTK_TAG_TYPES.WATCHLIST, RTK_TAG_TYPES.WATCHLIST_STATS],
    }),
    getWatchlistStatistics: builder.query<WatchlistStatistics, void>({
      query: () => API_ENDPOINTS.WATCHLIST.STATISTICS,
      providesTags: [RTK_TAG_TYPES.WATCHLIST_STATS],
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


