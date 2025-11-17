import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryWithReauth } from './baseApi';
import { MovieSearchResult, MovieDetails, MovieCredits, MovieVideo, Movie, CastMember, CrewMember } from '../../types/movie.types';
import { transformCastMember, transformCrewMember, transformVideo } from '../../utils/tmdbTransformers';
import { API_ENDPOINTS, RTK_REDUCER_PATHS, RTK_TAG_TYPES } from '../../utils/constants';

interface TmdbCastMemberRaw {
  id: number;
  cast_id?: number;
  castId?: number;
  character: string;
  name: string;
  profile_path?: string | null;
  profilePath?: string | null;
  order: number;
  gender: number;
  known_for_department?: string;
  knownForDepartment?: string;
}

interface TmdbCrewMemberRaw {
  id: number;
  credit_id?: string;
  creditId?: string;
  name: string;
  job: string;
  department: string;
  profile_path?: string | null;
  profilePath?: string | null;
  gender: number;
}

interface TmdbVideoRaw {
  id: string;
  key: string;
  name: string;
  site: string;
  type: string;
  official: boolean;
  published_at?: string;
  publishedAt?: string;
  size: number;
}

interface MovieDetailsApiResponse {
  tmdbId: number;
  title: string;
  overview: string;
  posterPath: string;
  backdropPath?: string | null;
  releaseDate: string;
  voteAverage: number;
  voteCount: number;
  popularity: number;
  genres: string[];
  creditsJson: {
    cast: TmdbCastMemberRaw[];
    crew: TmdbCrewMemberRaw[];
  };
  videosJson: TmdbVideoRaw[];
}

export const moviesApi = createApi({
  reducerPath: RTK_REDUCER_PATHS.MOVIES_API,
  baseQuery: baseQueryWithReauth,
  tagTypes: [RTK_TAG_TYPES.MOVIES, RTK_TAG_TYPES.MOVIE_DETAILS],
  endpoints: (builder) => ({
    searchMovies: builder.query<MovieSearchResult, { query: string; page?: number }>({
      query: ({ query, page = 1 }) => ({
        url: API_ENDPOINTS.MOVIES.SEARCH,
        params: { query, page },
      }),
      transformResponse: (response: Movie[]) => ({
        movies: Array.isArray(response) ? response : [],
        totalResults: Array.isArray(response) ? response.length : 0,
        totalPages: 1,
        currentPage: 1,
      }),
      providesTags: [RTK_TAG_TYPES.MOVIES],
    }),
    getPopularMovies: builder.query<MovieSearchResult, { page?: number }>({
      query: ({ page = 1 }) => ({
        url: API_ENDPOINTS.MOVIES.POPULAR,
        params: { page },
      }),
      transformResponse: (response: Movie[]) => ({
        movies: Array.isArray(response) ? response : [],
        totalResults: Array.isArray(response) ? response.length : 0,
        totalPages: 1,
        currentPage: 1,
      }),
      providesTags: [RTK_TAG_TYPES.MOVIES],
    }),
    getPopularMoviesInfinite: builder.query<MovieSearchResult[], { limit?: number }>({
      async queryFn({ limit = 10 }, _api, _extraOptions, fetchWithBQ) {
        const allResults: MovieSearchResult[] = [];
        
        for (let page = 1; page <= limit; page++) {
          const result = await fetchWithBQ({
            url: API_ENDPOINTS.MOVIES.POPULAR,
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
      providesTags: [RTK_TAG_TYPES.MOVIES],
    }),
    getMovieDetails: builder.query<{
      movie: MovieDetails;
      credits: MovieCredits;
      videos: MovieVideo[];
    }, { tmdbId: number }>({
      query: ({ tmdbId }) => API_ENDPOINTS.MOVIES.TMDB_DETAILS(tmdbId),
      transformResponse: (response: MovieDetailsApiResponse) => {
        const { creditsJson, videosJson, ...movieData } = response;
        return {
          movie: movieData as MovieDetails,
          credits: {
            cast: (creditsJson?.cast || []).map(transformCastMember),
            crew: (creditsJson?.crew || []).map(transformCrewMember),
          },
          videos: (videosJson || []).map(transformVideo),
        };
      },
      providesTags: (_result, _error, { tmdbId }) => [{ type: RTK_TAG_TYPES.MOVIE_DETAILS, id: tmdbId }],
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

