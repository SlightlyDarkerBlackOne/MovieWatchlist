import { createSelector } from '@reduxjs/toolkit';
import { RootState } from '../index';
import { watchlistApi } from '../api/watchlistApi';

export const selectWatchlistResult = (userId: number) =>
  watchlistApi.endpoints.getWatchlist.select(userId);

export const selectWatchlistByUserId = (state: RootState, userId: number) => {
  const result = selectWatchlistResult(userId)(state);
  return result?.data ?? null;
};

export const selectWatchlistMovieIds = createSelector(
  [
    (state: RootState, userId: number) => selectWatchlistResult(userId)(state),
  ],
  (watchlistResult) => {
    if (!watchlistResult?.data) return new Set<number>();
    return new Set(
      watchlistResult.data
        .map(item => item.movie?.tmdbId)
        .filter((id): id is number => id !== undefined)
    );
  }
);

export const createIsInWatchlistSelector = (userId: number, tmdbId: number) =>
  createSelector(
    [(state: RootState) => selectWatchlistMovieIds(state, userId)],
    (movieIds) => movieIds.has(tmdbId)
  );

