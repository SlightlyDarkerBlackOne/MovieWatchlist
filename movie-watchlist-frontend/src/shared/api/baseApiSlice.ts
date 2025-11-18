import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryWithReauth } from './baseApi';
import { RTK_TAG_TYPES } from '../constants/appConstants';

export const baseApiSlice = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithReauth,
  tagTypes: [
    RTK_TAG_TYPES.MOVIES,
    RTK_TAG_TYPES.MOVIE_DETAILS,
    RTK_TAG_TYPES.WATCHLIST,
    RTK_TAG_TYPES.WATCHLIST_STATS,
    RTK_TAG_TYPES.AUTH,
  ],
  endpoints: () => ({}),
});

