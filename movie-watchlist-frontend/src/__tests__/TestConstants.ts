/**
 * Test constants for frontend tests
 */

export const TestConstants = {
  Users: {
    DefaultUserId: 1,
    SecondUserId: 2,
    NonExistentUserId: 999,
  },

  Movies: {
    DefaultTmdbId: 550,
    SecondTmdbId: 155,
    ThirdTmdbId: 680,
    DefaultMovieId: 1,
    SecondMovieId: 2,
    ThirdMovieId: 3,
    DefaultTitle: 'Fight Club',
    SecondTitle: 'The Dark Knight',
    ThirdTitle: 'Pulp Fiction',
    DefaultOverview: 'A ticking-time-bomb insomniac and a slippery soap salesman channel primal male aggression into a shocking new form of therapy.',
  },

  Watchlist: {
    DefaultItemId: 1,
    SecondItemId: 2,
    DefaultNotes: 'Must watch!',
    SecondNotes: 'Great movie!',
    DefaultMovieId: 550,
  },

  ApiEndpoints: {
    WatchlistUser: '*/Watchlist/user/:userId',
    WatchlistAdd: '*/Watchlist/user/:userId/add',
    WatchlistItem: '*/Watchlist/user/:userId/item/:itemId',
    WatchlistStatistics: '*/Watchlist/user/:userId/statistics',
  },

  HttpStatusCodes: {
    Ok: 200,
    BadRequest: 400,
    Unauthorized: 401,
    NotFound: 404,
    InternalServerError: 500,
  },

  ErrorMessages: {
    FailedToLoad: 'Failed to load',
    SearchFailed: 'Search failed',
    FailedToFetchMovies: 'Failed to fetch movies',
    ServerError: 'Server error',
    MovieAlreadyInWatchlist: 'Movie is already in watchlist',
    MovieIdRequired: 'Movie ID is required',
    FailedToLoadPopularMovies: 'Failed to load popular movies',
  },

  SearchQueries: {
    Matrix: 'matrix',
    FightClub: 'fight club',
  },

  UI: {
    PopularMovies: 'Popular Movies',
    RefreshPopularMovies: 'refresh popular movies',
    Remove: 'Remove',
    NotSupported: 'Not supported',
  },

  TestValues: {
    FeaturedMoviesCount: 5,
    DefaultPage: 1,
    PollingIntervalMinutes: 3,
  },
} as const;

