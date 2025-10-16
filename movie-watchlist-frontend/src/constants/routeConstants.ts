/**
 * Route and View Constants
 * 
 * Centralizes all route paths and view names to prevent typos
 * and make refactoring easier.
 * 
 * public static class Routes { public const string LOGIN = "/login"; }
 */

// Auth view names (for state-based routing - will be replaced by React Router)
export const AUTH_VIEWS = {
  LOGIN: 'login',
  REGISTER: 'register',
  FORGOT_PASSWORD: 'forgot-password',
  RESET_PASSWORD: 'reset-password',
} as const;

// Route paths (for React Router)
export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',
  MOVIES: '/movies',
  MOVIE_DETAILS: (tmdbId: number) => `/movies/${tmdbId}`,
  WATCHLIST: '/watchlist',
} as const;

// Export types for TypeScript
export type AuthView = typeof AUTH_VIEWS[keyof typeof AUTH_VIEWS];
export type RoutePath = typeof ROUTES[keyof typeof ROUTES];

