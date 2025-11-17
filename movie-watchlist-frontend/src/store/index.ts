import { configureStore } from '@reduxjs/toolkit';
import { moviesApi } from '../features/movies/api/moviesApi';
import { watchlistApi } from '../features/watchlist/api/watchlistApi';
import { authApi } from '../features/auth/api/authApi';

export const store = configureStore({
  reducer: {
    [moviesApi.reducerPath]: moviesApi.reducer,
    [watchlistApi.reducerPath]: watchlistApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      moviesApi.middleware,
      watchlistApi.middleware,
      authApi.middleware
    ),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

