/**
 * Watchlist-related TypeScript interfaces
 */

export enum WatchlistStatus {
  Planned = 0,
  Watching = 1,
  Watched = 2,
  Dropped = 3,
}

export interface WatchlistItem {
  id: number;
  userId: number;
  movieId: number;
  movie?: {
    id: number;
    tmdbId: number;
    title: string;
    overview: string;
    releaseDate: string;
    posterPath: string | null;
    voteAverage: number;
    voteCount: number;
    genreIds: number[];
  };
  status: WatchlistStatus;
  userRating?: number;
  notes?: string;
  isFavorite: boolean;
  addedDate: string;
  watchedDate?: string;
}

export interface WatchlistStatistics {
  totalMovies: number;
  watchedMovies: number;
  plannedMovies: number;
  averageRating: number;
  mostWatchedGenre: string;
  genreBreakdown: { [genre: string]: number };
  totalWatchTime: number;
}

export interface AddToWatchlistRequest {
  movieId: number;
  status?: WatchlistStatus;
  notes?: string;
}

export interface UpdateWatchlistRequest {
  status?: WatchlistStatus;
  userRating?: number;
  notes?: string;
  isFavorite?: boolean;
  watchedDate?: string;
}

