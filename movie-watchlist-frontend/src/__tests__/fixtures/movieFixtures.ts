/**
 * Test fixtures for movie data
 */

import { Movie, MovieDetails, MovieVideo, CastMember, CrewMember, MovieCredits } from '../../types/movie.types';

export const mockMovie: Movie = {
  id: 1,
  tmdbId: 550,
  title: 'Fight Club',
  overview: 'A ticking-time-bomb insomniac and a slippery soap salesman channel primal male aggression into a shocking new form of therapy.',
  releaseDate: '1999-10-15',
  posterPath: '/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg',
  backdropPath: '/fCayJrkfRaCRCTh8GqN30f8oyQF.jpg',
  voteAverage: 8.4,
  voteCount: 26280,
  popularity: 61.416,
  genreIds: [18, 53, 35],
  originalLanguage: 'en',
  originalTitle: 'Fight Club',
  adult: false,
};

export const mockMovieWithoutPoster: Movie = {
  ...mockMovie,
  id: 2,
  tmdbId: 551,
  posterPath: null,
  backdropPath: null,
};

export const mockMovieDetails: MovieDetails = {
  ...mockMovie,
  runtime: 139,
  tagline: 'Mischief. Mayhem. Soap.',
  budget: 63000000,
  revenue: 100853753,
  status: 'Released',
  genres: ['Drama', 'Thriller', 'Comedy'],
  productionCompanies: [],
  spokenLanguages: [],
  imdbId: 'tt0137523',
};

export const mockCastMember: CastMember = {
  id: 287,
  castId: 4,
  character: 'Tyler Durden',
  name: 'Brad Pitt',
  profilePath: '/oTB9vGIBacH5aQNS0pUM74QSWuf.jpg',
  order: 0,
  gender: 2,
  knownForDepartment: 'Acting',
};

export const mockCrewMember: CrewMember = {
  id: 7467,
  creditId: '52fe4250c3a36847f80149f3',
  name: 'David Fincher',
  job: 'Director',
  department: 'Directing',
  profilePath: '/tpEczFclQZeKAiCeKZZ0adRvtfz.jpg',
  gender: 2,
};

export const mockMovieCredits: MovieCredits = {
  cast: [mockCastMember],
  crew: [mockCrewMember],
};

export const mockMovieVideo: MovieVideo = {
  id: '639d5326be6d88007f170f44',
  key: 'BdJKm16Co6M',
  name: 'Fight Club | #TBT Trailer | 20th Century FOX',
  site: 'YouTube',
  type: 'Trailer',
  official: true,
  publishedAt: '2016-07-13T01:29:26.000Z',
  size: 1080,
};

export const mockMovies: Movie[] = [
  mockMovie,
  {
    ...mockMovie,
    id: 2,
    tmdbId: 155,
    title: 'The Dark Knight',
    voteAverage: 8.5,
    voteCount: 28000,
  },
  {
    ...mockMovie,
    id: 3,
    tmdbId: 680,
    title: 'Pulp Fiction',
    voteAverage: 8.3,
    voteCount: 25000,
  },
];

