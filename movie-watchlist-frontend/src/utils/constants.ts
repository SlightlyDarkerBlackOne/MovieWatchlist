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
    BY_GENRE: (genre: string) => `/Movies/genre/${genre}`,
  },
  
  // Watchlist endpoints
  WATCHLIST: {
    USER: (userId: number) => `/Watchlist/user/${userId}`,
    ADD: (userId: number) => `/Watchlist/user/${userId}/add`,
    ITEM: (userId: number, itemId: number) => `/Watchlist/user/${userId}/item/${itemId}`,
    STATISTICS: (userId: number) => `/Watchlist/user/${userId}/statistics`,
    FAVORITES: (userId: number) => `/Watchlist/user/${userId}/favorites`,
    RECOMMENDATIONS: (userId: number) => `/Watchlist/user/${userId}/recommendations`,
    BY_GENRE: (userId: number, genre: string) => `/Watchlist/user/${userId}/genre/${genre}`,
    BY_YEAR_RANGE: (userId: number) => `/Watchlist/user/${userId}/year-range`,
    BY_STATUS: (userId: number, status: string) => `/Watchlist/user/${userId}/status/${status}`,
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

