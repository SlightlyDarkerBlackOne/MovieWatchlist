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
  UNEXPECTED_ERROR: 'An unexpected error occurred',
  SESSION_EXPIRED: 'Your session has expired. Please log in again.',
  SERVER_ERROR: 'Server error. Please try again later.',
  FAILED_TO_UPDATE_ITEM: 'Failed to update item',
  FAILED_TO_REMOVE_ITEM: 'Failed to remove item',
  AN_ERROR_OCCURRED: 'An error occurred',
  FAILED_TO_ADD_TO_WATCHLIST: 'Failed to add to watchlist',
  FAILED_TO_REMOVE_FROM_WATCHLIST: 'Failed to remove from watchlist',
  MOVIE_NOT_FOUND_IN_WATCHLIST: 'Movie not found in watchlist',
  MOVIE_NOT_FOUND: 'Movie not found',
} as const;

// Success Messages
export const SUCCESS_MESSAGES = {
  PASSWORD_RESET_EMAIL_SENT: 'Password reset email sent successfully',
  PASSWORD_RESET_SUCCESS: 'Password reset successfully',
  ADDED_TO_WATCHLIST: (title: string) => `Added "${title}" to your watchlist!`,
  REMOVED_FROM_WATCHLIST: (title: string) => `Removed "${title}" from your watchlist!`,
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

// HTTP Status Codes
export const HTTP_STATUS_CODES = {
  UNAUTHORIZED: 401,
  INTERNAL_SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503,
} as const;

// Route Paths
export const ROUTE_PATHS = {
  LOGIN: '/login',
} as const;

// HTTP Headers
export const HTTP_HEADERS = {
  CONTENT_TYPE: 'Content-Type',
} as const;

// HTTP Header Values
export const HTTP_HEADER_VALUES = {
  APPLICATION_JSON: 'application/json',
} as const;

// Fetch Credentials
export const FETCH_CREDENTIALS = {
  INCLUDE: 'include',
} as const;

// API Endpoint Patterns
export const API_ENDPOINT_PATTERNS = {
  AUTH: '/Auth/',
} as const;

// Default API Configuration
export const DEFAULT_API_CONFIG = {
  BASE_URL: 'http://localhost:5250/api',
} as const;

// Validation Constraints
export const VALIDATION_CONSTRAINTS = {
  USERNAME: {
    MIN_LENGTH: 3,
    MAX_LENGTH: 50,
  },
  EMAIL: {
    MIN_LENGTH: 5,
    MAX_LENGTH: 100,
  },
  PASSWORD: {
    MIN_LENGTH: 8,
    MAX_LENGTH: 100,
  },
  NOTES: {
    MAX_LENGTH: 1000,
  },
} as const;

// Validation Regex Patterns
export const VALIDATION_PATTERNS = {
  USERNAME: /^[a-zA-Z0-9_-]{3,50}$/,
  PASSWORD_UPPERCASE: /[A-Z]/,
  PASSWORD_LOWERCASE: /[a-z]/,
  PASSWORD_NUMBER: /[0-9]/,
  PASSWORD_SPECIAL: /[@$!%*?&]/,
} as const;

// Validation Error Messages
export const VALIDATION_MESSAGES = {
  USERNAME_OR_EMAIL_REQUIRED: 'Username or email is required',
  USERNAME_REQUIRED: 'Username is required',
  USERNAME_LENGTH: 'Username must be 3-50 characters',
  USERNAME_FORMAT: 'Username must be 3-50 characters and contain only letters, numbers, underscores, and hyphens',
  EMAIL_REQUIRED: 'Email is required',
  EMAIL_INVALID: 'Invalid email address',
  EMAIL_LENGTH_MIN: 'Email must be at least 5 characters',
  EMAIL_LENGTH_MAX: 'Email must be at most 100 characters',
  PASSWORD_REQUIRED: 'Password is required',
  PASSWORD_LENGTH_MIN: 'Password must be at least 8 characters',
  PASSWORD_LENGTH_MAX: 'Password must be at most 100 characters',
  PASSWORD_UPPERCASE: 'Password must contain at least one uppercase letter',
  PASSWORD_LOWERCASE: 'Password must contain at least one lowercase letter',
  PASSWORD_NUMBER: 'Password must contain at least one number',
  PASSWORD_SPECIAL: 'Password must contain at least one special character (@, $, !, %, *, ?, or &)',
  CONFIRM_PASSWORD_REQUIRED: 'Please confirm your password',
  PASSWORDS_DO_NOT_MATCH: 'Passwords do not match',
  NOTES_LENGTH_MAX: 'Notes must be at most 1000 characters',
} as const;

// Form Field Names
export const FORM_FIELD_NAMES = {
  CONFIRM_PASSWORD: 'confirmPassword',
} as const;

// Watchlist Page Text
export const WATCHLIST_PAGE_TEXT = {
  TITLE: 'My Watchlist',
  SUBTITLE: 'Manage your movie collection',
  LOGIN_REQUIRED: 'Please log in to view your watchlist.',
  GO_TO_LOGIN: 'Go to Login',
} as const;

// Watchlist Tab Labels
export const WATCHLIST_TAB_LABELS = {
  ALL: 'All',
  FAVORITES: 'Favorites',
  WATCHED: 'Watched',
} as const;

// Watchlist Filter Values
export const WATCHLIST_FILTER_VALUES = {
  ALL: 'all',
} as const;

// Movie Details Page Text
export const MOVIE_DETAILS_PAGE_TEXT = {
  GO_BACK: 'Go Back',
} as const;

// Default Values
export const DEFAULT_VALUES = {
  TMDB_ID: 0,
} as const;

// Search Constants
export const SEARCH_CONSTANTS = {
  MIN_QUERY_LENGTH: 2,
  MAX_DROPDOWN_RESULTS: 8,
  DEBOUNCE_DELAY_MS: 500,
  PLACEHOLDER: 'Search movies…',
  MIN_CHARACTERS_MESSAGE: 'Type at least 2 characters to search',
  NO_RESULTS_MESSAGE: 'No movies found',
  TRY_SEARCHING_ELSE: 'Try searching for something else',
} as const;

// Infinite List Constants
export const INFINITE_LIST_CONSTANTS = {
  DEFAULT_LOADING_MESSAGE: 'Loading more movies...',
  DEFAULT_EMPTY_MESSAGE: 'No movies found',
  ALL_LOADED_MESSAGE: 'All movies loaded',
  INTERSECTION_THRESHOLD: 0.1,
} as const;

// Header Auth Button Text
export const HEADER_AUTH_BUTTON_TEXT = {
  MY_WATCHLIST: 'My Watchlist',
  LOGIN: 'Login',
  LOGOUT: 'Logout',
} as const;

// Trailer Section Constants
export const TRAILER_CONSTANTS = {
  ASPECT_RATIO: '16 / 9',
  MAX_HEIGHT: 'calc(100vh - 20px)',
  SCROLL_BEHAVIOR: 'smooth',
  SCROLL_BLOCK: 'center',
} as const;

// Keyboard Keys
export const KEYBOARD_KEYS = {
  ENTER: 'Enter',
} as const;

// Year Display
export const YEAR_DISPLAY = {
  NOT_AVAILABLE: 'N/A',
} as const;

// Poster Sizes
export const POSTER_SIZES = {
  SMALL: 'small',
} as const;

// UI Constants
export const UI_CONSTANTS = {
  SNACKBAR: {
    AUTO_HIDE_DURATION: 5000,
    SUCCESS_AUTO_HIDE_DURATION: 3000,
    ANCHOR_ORIGIN: {
      VERTICAL_TOP: 'top',
      HORIZONTAL_CENTER: 'center',
    },
  },
  LOADING_SPINNER: {
    SIZE: 60,
  },
  TIMEOUT: {
    SUCCESS_MESSAGE: 3000,
  },
  CAST_SLICE: {
    TOP_COUNT: 10,
  },
  ALERT_SEVERITY: {
    ERROR: 'error',
    WARNING: 'warning',
    SUCCESS: 'success',
  },
  ARIA_ROLES: {
    TABPANEL: 'tabpanel',
    TAB: 'tab',
  },
  ID_PREFIXES: {
    WATCHLIST_TABPANEL: 'watchlist-tabpanel-',
    WATCHLIST_TAB: 'watchlist-tab-',
  },
  ARIA_LABELS: {
    WATCHLIST_TABS: 'watchlist tabs',
  },
  ARIA_LIVE: {
    POLITE: 'polite',
  },
  ARIA_ROLE: {
    STATUS: 'status',
  },
  CSS_CLASSES: {
    SCREEN_READER_ONLY: 'sr-only',
  },
  CIRCULAR_PROGRESS: {
    SMALL_SIZE: 20,
  },
} as const;


