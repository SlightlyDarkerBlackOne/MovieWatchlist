import { createSelector } from '@reduxjs/toolkit';
import { RootState } from '../index';
import { watchlistApi } from '../api/watchlistApi';

export const selectWatchlistResult = () =>
  watchlistApi.endpoints.getWatchlist.select(undefined);

export const selectWatchlistByUserId = (state: RootState) => {
  const result = selectWatchlistResult()(state);
  return result?.data ?? null;
};

export const selectWatchlistMovieIds = createSelector(
  [
    (state: RootState) => selectWatchlistResult()(state),
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

export const createIsInWatchlistSelector = (tmdbId: number) =>
  createSelector(
    [(state: RootState) => selectWatchlistMovieIds(state)],
    (movieIds) => movieIds.has(tmdbId)
  );

