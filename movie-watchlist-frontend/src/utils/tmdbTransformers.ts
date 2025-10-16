/**
 * Utility functions for transforming TMDB API responses
 * Converts snake_case to camelCase for consistency with frontend code
 */

import { CastMember, CrewMember, MovieVideo } from '../types/movie.types';

// Raw TMDB API response types (snake_case)
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

/**
 * Transform TMDB cast member from snake_case to camelCase
 */
export const transformCastMember = (member: TmdbCastMemberRaw): CastMember => ({
  id: member.id,
  castId: member.cast_id ?? member.castId ?? 0,
  character: member.character,
  name: member.name,
  profilePath: member.profile_path ?? member.profilePath ?? null,
  order: member.order,
  gender: member.gender,
  knownForDepartment: member.known_for_department ?? member.knownForDepartment ?? '',
});

/**
 * Transform TMDB crew member from snake_case to camelCase
 */
export const transformCrewMember = (member: TmdbCrewMemberRaw): CrewMember => ({
  id: member.id,
  creditId: member.credit_id ?? member.creditId ?? '',
  name: member.name,
  job: member.job,
  department: member.department,
  profilePath: member.profile_path ?? member.profilePath ?? null,
  gender: member.gender,
});

/**
 * Transform TMDB video from snake_case to camelCase
 */
export const transformVideo = (video: TmdbVideoRaw): MovieVideo => ({
  id: video.id,
  key: video.key,
  name: video.name,
  site: video.site,
  type: video.type,
  official: video.official,
  publishedAt: video.published_at ?? video.publishedAt ?? '',
  size: video.size,
});

