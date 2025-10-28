import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { 
  searchMovies, 
  getPopularMovies, 
  getMovieDetailsByTmdbId 
} from '../../services/movieService';
import { MovieSearchResult } from '../../types/movie.types';

const baseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5250/api';

export const moviesApi = createApi({
  reducerPath: 'moviesApi',
  baseQuery: fetchBaseQuery({ baseUrl }),
  tagTypes: ['Movies', 'MovieDetails'],
  endpoints: (builder) => ({
    searchMovies: builder.query<MovieSearchResult, { query: string; page?: number }>({
      async queryFn({ query, page = 1 }) {
        try {
          const result = await searchMovies(query, page);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      providesTags: ['Movies'],
    }),
    getPopularMovies: builder.query<MovieSearchResult, { page?: number }>({
      async queryFn({ page = 1 }) {
        try {
          const result = await getPopularMovies(page);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      providesTags: ['Movies'],
    }),
    getPopularMoviesInfinite: builder.query<MovieSearchResult[], { limit?: number }>({
      async queryFn({ limit = 10 }) {
        try {
          const allResults: MovieSearchResult[] = [];
          for (let page = 1; page <= limit; page++) {
            const result = await getPopularMovies(page);
            allResults.push(result);
          }
          return { data: allResults };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      serializeQueryArgs: ({ endpointName }) => {
        return endpointName;
      },
      merge: (currentCache, newItems) => {
        // Get all tmdbIds already in cache
        const seenTmdbIds = new Set<number>();
        currentCache.forEach(page => {
          page.movies.forEach(movie => {
            seenTmdbIds.add(movie.tmdbId);
          });
        });

        // Filter new items to only include unique movies
        const deduplicatedNewItems = newItems.map(page => ({
          ...page,
          movies: page.movies.filter(movie => {
            if (seenTmdbIds.has(movie.tmdbId)) {
              return false;
            }
            seenTmdbIds.add(movie.tmdbId);
            return true;
          })
        })).filter(page => page.movies.length > 0);

        return [...currentCache, ...deduplicatedNewItems];
      },
      forceRefetch({ currentArg, previousArg }) {
        return currentArg?.limit !== previousArg?.limit;
      },
      providesTags: ['Movies'],
    }),
    getMovieDetails: builder.query<{
      movie: any;
      credits: any;
      videos: any;
    }, { tmdbId: number }>({
      async queryFn({ tmdbId }) {
        try {
          const result = await getMovieDetailsByTmdbId(tmdbId);
          return { data: result };
        } catch (error) {
          return { error: { status: 'CUSTOM_ERROR', error: String(error) } };
        }
      },
      providesTags: (_result, _error, { tmdbId }) => [{ type: 'MovieDetails', id: tmdbId }],
    }),
  }),
});

export const { 
  useSearchMoviesQuery, 
  useGetPopularMoviesQuery, 
  useGetPopularMoviesInfiniteQuery,
  useGetMovieDetailsQuery 
} = moviesApi;

