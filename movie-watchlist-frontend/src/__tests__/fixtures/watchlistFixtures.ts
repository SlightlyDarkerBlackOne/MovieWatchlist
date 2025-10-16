/**
 * Test fixtures for watchlist data
 */

import { WatchlistItem, WatchlistStatus } from '../../types/watchlist.types';
import { mockMovie, mockMovies } from './movieFixtures';

export const mockWatchlistItem: WatchlistItem = {
  id: 1,
  userId: 1,
  movieId: 1,
  movie: {
    id: mockMovie.id,
    tmdbId: mockMovie.tmdbId,
    title: mockMovie.title,
    overview: mockMovie.overview,
    releaseDate: mockMovie.releaseDate,
    posterPath: mockMovie.posterPath,
    voteAverage: mockMovie.voteAverage,
    voteCount: mockMovie.voteCount,
    genreIds: [],
  },
  status: WatchlistStatus.Planned,
  userRating: undefined,
  notes: undefined,
  isFavorite: false,
  addedDate: '2024-01-15T10:30:00Z',
  watchedDate: undefined,
};

export const mockWatchlistItemWatched: WatchlistItem = {
  ...mockWatchlistItem,
  id: 2,
  movieId: 2,
  movie: {
    id: mockMovies[1].id,
    tmdbId: mockMovies[1].tmdbId,
    title: mockMovies[1].title,
    overview: mockMovies[1].overview,
    releaseDate: mockMovies[1].releaseDate,
    posterPath: mockMovies[1].posterPath,
    voteAverage: mockMovies[1].voteAverage,
    voteCount: mockMovies[1].voteCount,
    genreIds: [],
  },
  status: WatchlistStatus.Watched,
  userRating: 4.5,
  notes: 'Great movie!',
  isFavorite: true,
  addedDate: '2024-01-10T08:00:00Z',
  watchedDate: '2024-01-12T20:30:00Z',
};

export const mockWatchlistItems: WatchlistItem[] = [
  mockWatchlistItem,
  mockWatchlistItemWatched,
  {
    ...mockWatchlistItem,
    id: 3,
    movieId: 3,
    movie: {
      id: mockMovies[2].id,
      tmdbId: mockMovies[2].tmdbId,
      title: mockMovies[2].title,
      overview: mockMovies[2].overview,
      releaseDate: mockMovies[2].releaseDate,
      posterPath: mockMovies[2].posterPath,
      voteAverage: mockMovies[2].voteAverage,
      voteCount: mockMovies[2].voteCount,
      genreIds: [],
    },
    status: WatchlistStatus.Watching,
    isFavorite: false,
    addedDate: '2024-01-20T15:45:00Z',
  },
];


