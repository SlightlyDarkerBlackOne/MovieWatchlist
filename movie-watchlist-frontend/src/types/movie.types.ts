/**
 * Movie-related TypeScript interfaces
 */

export interface Movie {
  id: number;
  tmdbId: number;
  title: string;
  overview: string;
  releaseDate: string;
  posterPath: string | null;
  backdropPath: string | null;
  voteAverage: number;
  voteCount: number;
  popularity: number;
  genreIds: number[];
  originalLanguage: string;
  originalTitle: string;
  adult: boolean;
}

export interface MovieSearchParams {
  query?: string;
  page?: number;
  genre?: string;
  year?: number;
}

export interface MovieSearchResult {
  movies: Movie[];
  totalResults: number;
  totalPages: number;
  currentPage: number;
}

export interface Genre {
  id: number;
  name: string;
}

export interface MovieDetails extends Movie {
  runtime: number | null;
  tagline: string | null;
  budget: number;
  revenue: number;
  status: string;
  genres: string[];
  productionCompanies: ProductionCompany[];
  spokenLanguages: SpokenLanguage[];
  imdbId: string | null;
}

export interface MovieVideo {
  id: string;
  key: string; // YouTube video key
  name: string;
  site: string; // "YouTube"
  type: string; // "Trailer", "Teaser", "Clip", etc.
  official: boolean;
  publishedAt: string;
  size: number; // 1080, 720, 480, etc.
}

export interface CastMember {
  id: number;
  castId: number;
  character: string;
  name: string;
  profilePath: string | null;
  order: number;
  gender: number;
  knownForDepartment: string;
}

export interface CrewMember {
  id: number;
  creditId: string;
  name: string;
  job: string;
  department: string;
  profilePath: string | null;
  gender: number;
}

export interface MovieCredits {
  cast: CastMember[];
  crew: CrewMember[];
}

export interface ProductionCompany {
  id: number;
  name: string;
  logoPath: string | null;
  originCountry: string;
}

export interface SpokenLanguage {
  englishName: string;
  iso6391: string;
  name: string;
}

