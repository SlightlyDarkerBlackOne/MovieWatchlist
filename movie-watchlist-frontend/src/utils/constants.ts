// API Endpoints
export const API_ENDPOINTS = {
  // Auth endpoints
  AUTH: {
    LOGIN: '/Auth/login',
    REGISTER: '/Auth/register',
    LOGOUT: '/Auth/logout',
    REFRESH: '/Auth/refresh',
    ME: '/Auth/me',
    FORGOT_PASSWORD: '/Auth/forgot-password',
    RESET_PASSWORD: '/Auth/reset-password',
  },
  
  // Movie endpoints
  MOVIES: {
    SEARCH: '/Movies/search',
    POPULAR: '/Movies/popular',
    DETAILS: (id: number) => `/Movies/${id}`,
    TMDB_DETAILS: (tmdbId: number) => `/Movies/tmdb/${tmdbId}`,
    BY_GENRE: (genre: string) => `/Movies/genre/${genre}`,
  },
  
  // Watchlist endpoints
  WATCHLIST: {
    ME: '/Watchlist/me/watchlist',
    ADD: '/Watchlist/me/watchlist/add',
    UPDATE_ITEM: '/Watchlist/me/watchlist/item',
    ITEM: (itemId: number) => `/Watchlist/me/watchlist/item/${itemId}`,
    STATISTICS: '/Watchlist/me/watchlist/statistics',
    FAVORITES: '/Watchlist/me/watchlist/favorites',
    RECOMMENDATIONS: '/Watchlist/me/watchlist/recommendations',
    BY_GENRE: (genre: string) => `/Watchlist/me/watchlist/genre/${genre}`,
    BY_YEAR_RANGE: '/Watchlist/me/watchlist/year-range',
    BY_STATUS: (status: string) => `/Watchlist/me/watchlist/status/${status}`,
  },
};

// Local storage keys
export const STORAGE_KEYS = {} as const;

// App configuration
export const APP_CONFIG = {
  API_TIMEOUT: 10000, // 10 seconds
  DEFAULT_PAGE_SIZE: 20,
  MAX_RETRY_ATTEMPTS: 3,
};

// Cache configuration
export const CACHE_CONFIG = {
  POPULAR_MOVIES_CACHE_MINUTES: 3, // Cache popular movies for 3 minutes
  SEARCH_CACHE_TTL_MINUTES: 30, // Cache search results for 30 minutes
  MOVIE_DETAILS_CACHE_HOURS: 1, // Cache movie details for 1 hour
  CACHE_VERSION: '1.0',
  MAX_CACHE_SIZE_MB: 5,
};

// Cache keys
export const CACHE_KEYS = {
  POPULAR_MOVIES: 'popular_movies',
  POPULAR_MOVIES_PREFIX: 'popular_movies_page_',
  MOVIE_DETAILS: 'movie_details',
  SEARCH_RESULTS: 'search_results',
};

// TMDB Image Configuration
export const TMDB_IMAGE_CONFIG = {
  BASE_URL: 'https://image.tmdb.org/t/p',
  PROFILE_SIZES: {
    SMALL: 'w185',
    MEDIUM: 'h632',
    LARGE: 'original',
  },
};

// Error Messages
export const ERROR_MESSAGES = {
  UNKNOWN_ERROR: 'An unknown error occurred',
  NETWORK_ERROR: 'Network error. Please check your connection.',
  PARSING_ERROR: 'Failed to parse server response.',
  FAILED_TO_SEND_RESET_EMAIL: 'Failed to send reset email',
  FAILED_TO_RESET_PASSWORD: 'Failed to reset password',
} as const;

// RTK Query Error Status Values
export const RTK_QUERY_ERROR_STATUS = {
  FETCH_ERROR: 'FETCH_ERROR',
  PARSING_ERROR: 'PARSING_ERROR',
} as const;

// Error Object Property Names
export const ERROR_PROPERTIES = {
  STATUS: 'status',
  DATA: 'data',
  MESSAGE: 'message',
} as const;

// JavaScript Typeof Values
export const TYPE_OF_VALUES = {
  OBJECT: 'object',
  STRING: 'string',
} as const;

// HTTP Methods
export const HTTP_METHODS = {
  GET: 'GET',
  POST: 'POST',
  PUT: 'PUT',
  DELETE: 'DELETE',
  PATCH: 'PATCH',
} as const;

// RTK Query Reducer Paths
export const RTK_REDUCER_PATHS = {
  MOVIES_API: 'moviesApi',
  WATCHLIST_API: 'watchlistApi',
  AUTH_API: 'authApi',
} as const;

// RTK Query Tag Types
export const RTK_TAG_TYPES = {
  MOVIES: 'Movies',
  MOVIE_DETAILS: 'MovieDetails',
  WATCHLIST: 'Watchlist',
  WATCHLIST_STATS: 'WatchlistStats',
  AUTH: 'Auth',
} as const;

