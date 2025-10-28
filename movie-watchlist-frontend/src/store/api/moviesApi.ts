import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { MovieSearchResult, MovieDetails, MovieCredits, MovieVideo, Movie } from '../../types/movie.types';
import { transformCastMember, transformCrewMember, transformVideo } from '../../utils/tmdbTransformers';
import { STORAGE_KEYS } from '../../utils/constants';

const baseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5250/api';

export const moviesApi = createApi({
  reducerPath: 'moviesApi',
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
  tagTypes: ['Movies', 'MovieDetails'],
  endpoints: (builder) => ({
    searchMovies: builder.query<MovieSearchResult, { query: string; page?: number }>({
      query: ({ query, page = 1 }) => ({
        url: '/Movies/search',
        params: { query, page },
      }),
      transformResponse: (response: Movie[]) => ({
        movies: Array.isArray(response) ? response : [],
        totalResults: Array.isArray(response) ? response.length : 0,
        totalPages: 1,
        currentPage: 1,
      }),
      providesTags: ['Movies'],
    }),
    getPopularMovies: builder.query<MovieSearchResult, { page?: number }>({
      query: ({ page = 1 }) => ({
        url: '/Movies/popular',
        params: { page },
      }),
      transformResponse: (response: Movie[]) => ({
        movies: Array.isArray(response) ? response : [],
        totalResults: Array.isArray(response) ? response.length : 0,
        totalPages: 1,
        currentPage: 1,
      }),
      providesTags: ['Movies'],
    }),
    getPopularMoviesInfinite: builder.query<MovieSearchResult[], { limit?: number }>({
      async queryFn({ limit = 10 }, _api, _extraOptions, fetchWithBQ) {
        const allResults: MovieSearchResult[] = [];
        
        for (let page = 1; page <= limit; page++) {
          const result = await fetchWithBQ({
            url: '/Movies/popular',
            params: { page },
          });
          
          if (result.error) {
            return { error: result.error };
          }
          
          const movies = Array.isArray(result.data) ? result.data as Movie[] : [];
          allResults.push({
            movies,
            totalResults: movies.length,
            totalPages: 1,
            currentPage: page,
          });
        }
        
        return { data: allResults };
      },
      serializeQueryArgs: ({ endpointName }) => {
        return endpointName;
      },
      merge: (currentCache, newItems) => {
        const seenTmdbIds = new Set<number>();
        currentCache.forEach(page => {
          page.movies.forEach(movie => {
            seenTmdbIds.add(movie.tmdbId);
          });
        });

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
      movie: MovieDetails;
      credits: MovieCredits;
      videos: MovieVideo[];
    }, { tmdbId: number }>({
      query: ({ tmdbId }) => `/Movies/tmdb/${tmdbId}`,
      transformResponse: (response: {
        movie: MovieDetails;
        credits: { cast: any[]; crew: any[] };
        videos: any[];
      }) => ({
        movie: response.movie,
        credits: {
          cast: (response.credits?.cast || []).map(transformCastMember),
          crew: (response.credits?.crew || []).map(transformCrewMember),
        },
        videos: (response.videos || []).map(transformVideo),
      }),
      providesTags: (_result, _error, { tmdbId }) => [{ type: 'MovieDetails', id: tmdbId }],
    }),
  }),
});

export const { 
  useSearchMoviesQuery,
  useLazySearchMoviesQuery,
  useGetPopularMoviesQuery, 
  useGetPopularMoviesInfiniteQuery,
  useGetMovieDetailsQuery 
} = moviesApi;

