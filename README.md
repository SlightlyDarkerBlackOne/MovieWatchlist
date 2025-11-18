# MovieWatchlist

A full-stack movie watchlist application with TMDB integration, built with .NET 9 backend, React frontend, and PostgreSQL database

## Description

A comprehensive movie management platform that allows users to discover, track, and organize their movie watchlists. Features include movie search, personal watchlist management, and genre-based recommendations.

## Structure

    MovieWatchlist # Solution
    |
    ├── MovieWatchlist.Api/              # Presentation Layer
    │   ├── Controllers/                 # API Controllers (HTTP endpoints)
    │   │   ├── AuthController          # Authentication endpoints (MediatR)
    │   │   ├── BaseApiController       # Base controller with token helpers
    │   │   ├── MoviesController        # Movie search and details (MediatR)
    │   │   └── WatchlistController     # Watchlist management (MediatR)
    │   ├── Mapping/                     # Mapster mapping profiles
    │   │   └── AuthMappingProfile      # Auth DTO mappings
    │   ├── Middleware/                 # Custom middleware
    │   │   ├── GlobalExceptionMiddleware # Error handling
    │   │   └── RateLimitingMiddleware  # Rate limiting
    │   ├── Extensions/                  # Extension methods
    │   │   └── ClaimsPrincipalExtensions # Claims helper extensions
    │   ├── Helpers/                     # Helper classes
    │   │   └── AuthCookieManager       # Cookie management for auth
    │   ├── Options/                      # Options pattern classes
    │   │   └── AuthCookieOptions        # Cookie configuration options
    │   ├── Services/                     # API-level services
    │   │   ├── AuthCookieService      # Cookie management implementation
    │   │   ├── CurrentUserService     # Current user context service
    │   │   └── TokenExtractor         # Token extraction implementation
    │   ├── Constants/                  # Configuration constants
    │   │   ├── ConfigurationConstants  # Configuration constants
    │   │   ├── EnvironmentVariables    # Environment variable names
    │   │   └── MiddlewareConstants     # Middleware constants
    │   └── Program.cs                  # Application entry point & DI setup
    |
    ├── MovieWatchlist.Application/      # Application Layer (Business Logic)
    │   ├── Features/                    # Feature-based organization (CQRS)
    │   │   ├── Auth/                    # Authentication feature
    │   │   │   ├── Commands/            # Auth commands
    │   │   │   │   ├── CreateRefreshToken/ # Command + Handler + DTOs
    │   │   │   │   ├── ForgotPassword/
    │   │   │   │   ├── Login/
    │   │   │   │   ├── Logout/
    │   │   │   │   ├── RefreshToken/
    │   │   │   │   ├── Register/
    │   │   │   │   ├── ResetPassword/
    │   │   │   │   └── ValidateToken/
    │   │   │   ├── Queries/             # Auth queries
    │   │   │   │   └── GetCurrentUser/ # Query + Handler
    │   │   │   └── Common/              # Shared auth types
    │   │   │       ├── AuthenticationResult
    │   │   │       └── UserInfo
    │   │   ├── Movies/                  # Movies feature
    │   │   │   ├── Queries/             # Movie queries
    │   │   │   │   ├── GetMovieDetails/
    │   │   │   │   ├── GetMovieDetailsByTmdbId/
    │   │   │   │   ├── GetMoviesByGenre/
    │   │   │   │   ├── GetPopularMovies/
    │   │   │   │   └── SearchMovies/
    │   │   │   └── Common/              # Shared movie types
    │   │   │       ├── MovieDetailsDto
    │   │   │       └── MovieMappingProfile
    │   │   └── Watchlist/               # Watchlist feature
    │   │       ├── Commands/            # Watchlist commands
    │   │       │   ├── AddToWatchlist/
    │   │       │   ├── RemoveFromWatchlist/
    │   │       │   └── UpdateWatchlistItem/
    │   │       └── Queries/             # Watchlist queries
    │   │           ├── GetMyFavoriteMovies/
    │   │           ├── GetMyRecommendedMovies/
    │   │           ├── GetMyStatistics/
    │   │           ├── GetMyWatchlist/
    │   │           ├── GetMyWatchlistByGenre/
    │   │           ├── GetMyWatchlistByRatingRange/
    │   │           ├── GetMyWatchlistByStatus/
    │   │           ├── GetMyWatchlistByYearRange/
    │   │           └── GetMyWatchlistItemById/
    │   ├── Behaviors/                   # MediatR pipeline behaviors
    │   │   ├── ResultFailureBehavior    # Converts Result failures to exceptions
    │   │   └── TransactionBehavior      # Unit of Work per request
    │   ├── Interfaces/                  # Application service interfaces
    │   │   ├── IAuthenticationService  # Auth service interface
    │   │   └── IWatchlistService       # Watchlist service interface
    │   ├── Services/                    # Business logic implementations
    │   │   ├── AuthenticationService    # User authentication & authorization
    │   │   └── WatchlistService         # Watchlist business rules
    │   └── Events/                      # Domain event handling
    │       └── Handlers/                # Event handlers
    │           └── UpdateStatisticsHandler # Update statistics on watchlist changes
    |
    ├── MovieWatchlist.Core/            # Domain Layer (Core Business)
    │   ├── Configuration/              # Configuration models
    │   │   └── JwtSettings            # JWT configuration
    │   ├── Interfaces/                 # Service contracts (abstractions)
    │   │   ├── ICurrentUserService     # Current user context interface
    │   │   ├── IRepository            # Generic repository interface
    │   │   ├── IMovieRepository       # Movie data access interface
    │   │   ├── IWatchlistRepository   # Watchlist data access interface
    │   │   ├── IUserRepository        # User data access interface
    │   │   ├── IRefreshTokenRepository # Refresh token repository
    │   │   ├── IPasswordResetTokenRepository # Password reset repository
    │   │   ├── ITmdbService           # TMDB integration interface
    │   │   ├── IEmailService          # Email service interface
    │   │   ├── IGenreService          # Genre service interface
    │   │   ├── IJwtTokenService       # JWT token service interface
    │   │   ├── IPasswordHasher        # Password hashing interface
    │   │   ├── IUnitOfWork            # Transaction management interface
    │   │   ├── IRetryPolicyService    # Retry policy interface
    │   │   ├── IDomainEventDispatcher # Domain event dispatcher
    │   │   ├── IDomainEventHandler    # Domain event handler
    │   │   ├── ITokenExtractor        # Token extraction abstraction
    │   │   └── IAuthCookieService     # Cookie management abstraction
    │   ├── Models/                     # Domain entities (business objects)
    │   │   ├── Entity                 # Base entity with domain events
    │   │   ├── User                   # User entity
    │   │   ├── Movie                  # Movie entity (cached from TMDB)
    │   │   ├── WatchlistItem          # Watchlist entry
    │   │   ├── WatchlistStatistics    # Watchlist statistics aggregate
    │   │   ├── RefreshToken           # JWT refresh token
    │   │   └── PasswordResetToken     # Password reset token
    │   ├── ValueObjects/               # Domain value objects
    │   │   ├── Email                  # Email value object
    │   │   ├── Password               # Password value object
    │   │   ├── Rating                 # Rating value object
    │   │   └── Username               # Username value object
    │   ├── Events/                     # Domain events
    │   │   ├── IDomainEvent           # Domain event interface
    │   │   ├── AuthenticationEvents   # Authentication domain events
    │   │   └── WatchlistEvents        # Watchlist domain events
    │   ├── Specifications/             # Business rules (Specification pattern)
    │   │   ├── Specification          # Specification base class
    │   │   └── WatchlistSpecifications # Watchlist business rules
    │   ├── Common/                     # Shared abstractions
    │   │   └── Result                 # Result pattern for error handling
    │   ├── Exceptions/                 # Custom exceptions
    │   │   └── ApiException           # Custom exception hierarchy (ApiException, ValidationException, AuthenticationException, AuthorizationException, NotFoundException, ConflictException, RateLimitException, ExternalServiceException)
    │   └── Constants/                  # Domain constants
    │       ├── CookieNames            # Cookie name constants
    │       ├── ErrorCodes             # Error code constants
    │       ├── ErrorMessages          # Centralized error messages
    │       ├── GenreConstants         # Movie genre definitions
    │       ├── SuccessMessages        # Success message constants
    │       └── ValidationConstants    # Validation rules
    |
    ├── MovieWatchlist.Persistence/     # Persistence Layer (Data Access)
    │   ├── Data/                       # Database context
    │   │   ├── MovieWatchlistDbContext # EF Core context
    │   │   └── MovieWatchlistDbContextFactory # Design-time factory
    │   ├── Repositories/               # Data access implementations
    │   │   ├── EfRepository           # Generic repository (EF Core)
    │   │   ├── UnitOfWork             # Transaction management & domain events
    │   │   ├── UserRepository         # User data access
    │   │   ├── MovieRepository        # Movie data access
    │   │   ├── WatchlistRepository    # Watchlist data access
    │   │   ├── RefreshTokenRepository # Refresh token repository
    │   │   ├── PasswordResetTokenRepository # Password reset repository
    │   │   └── InMemoryRepository     # In-memory repository for testing
    │   └── Migrations/                 # EF Core migrations
    │       ├── InitialCreate          # Initial database schema
    │       ├── AddPasswordResetToken  # Password reset support
    │       ├── AddCreditsAndVideosToMovie # Movie credits/videos caching
    │       ├── AddValueObjectsSupport # Value object support
    │       └── AddUserStatisticsAndNullableRating # Statistics caching
    |
    ├── MovieWatchlist.Infrastructure/  # Infrastructure Layer (External Concerns)
    │   ├── Configuration/              # Infrastructure configuration
    │   │   └── TmdbSettings           # TMDB API settings
    │   ├── Services/                   # External service implementations
    │   │   ├── TmdbService            # TMDB API integration
    │   │   ├── EmailService           # Email sending (SMTP)
    │   │   ├── GenreService           # Genre mapping service
    │   │   ├── JwtTokenService        # JWT token generation/validation
    │   │   ├── PasswordHasher         # Password hashing (PBKDF2)
    │   │   └── RetryPolicyService     # Retry policy implementation
    │   ├── Behaviors/                  # Infrastructure behaviors
    │   │   └── LoggingBehavior         # Request/response logging
    │   ├── Events/                     # Domain event implementations
    │   │   ├── DomainEventDispatcher   # Event dispatcher implementation
    │   │   ├── LogActivityHandler     # Log domain events
    │   │   ├── RefreshTokenCreatedEventHandler
    │   │   ├── UserLoggedInEventHandler
    │   │   ├── UserPasswordChangedEventHandler
    │   │   └── UserRegisteredEventHandler
    │   └── DTOs/                       # Infrastructure-specific DTOs
    │       └── TmdbMovieDto           # TMDB API response models
    |
    ├── MovieWatchlist.Application.UnitTests/  # Application layer unit tests
    │   ├── Features/                  # Handler tests (CQRS)
    │   │   ├── Auth/                  # Auth handler tests
    │   │   │   ├── Commands/         # Command handler tests
    │   │   │   │   ├── CreateRefreshTokenCommandHandlerTests
    │   │   │   │   ├── ForgotPasswordCommandHandlerTests
    │   │   │   │   ├── LoginCommandHandlerTests
    │   │   │   │   ├── LogoutCommandHandlerTests
    │   │   │   │   ├── RefreshTokenCommandHandlerTests
    │   │   │   │   ├── RegisterCommandHandlerTests
    │   │   │   │   ├── ResetPasswordCommandHandlerTests
    │   │   │   │   └── ValidateTokenCommandHandlerTests
    │   │   │   └── Queries/          # Query handler tests
    │   │   │       └── GetCurrentUserQueryHandlerTests
    │   │   ├── Movies/               # Movie handler tests
    │   │   │   └── Queries/          # Movie query handler tests
    │   │   │       ├── GetMovieDetailsByTmdbIdQueryHandlerTests
    │   │   │       ├── GetMovieDetailsQueryHandlerTests
    │   │   │       ├── GetMoviesByGenreQueryHandlerTests
    │   │   │       ├── GetPopularMoviesQueryHandlerTests
    │   │   │       └── SearchMoviesQueryHandlerTests
    │   │   └── Watchlist/            # Watchlist handler tests
    │   │       ├── Commands/         # Watchlist command handler tests
    │   │       │   ├── AddToWatchlistCommandHandlerTests
    │   │       │   ├── RemoveFromWatchlistCommandHandlerTests
    │   │       │   └── UpdateWatchlistItemCommandHandlerTests
    │   │       └── Queries/          # Watchlist query handler tests
    │   │           ├── GetMyFavoriteMoviesQueryHandlerTests
    │   │           ├── GetMyRecommendedMoviesQueryHandlerTests
    │   │           ├── GetMyStatisticsQueryHandlerTests
    │   │           ├── GetMyWatchlistByGenreQueryHandlerTests
    │   │           ├── GetMyWatchlistByRatingRangeQueryHandlerTests
    │   │           ├── GetMyWatchlistByStatusQueryHandlerTests
    │   │           ├── GetMyWatchlistByYearRangeQueryHandlerTests
    │   │           ├── GetMyWatchlistItemByIdQueryHandlerTests
    │   │           └── GetMyWatchlistQueryHandlerTests
    │   ├── Events/                    # Event handler tests
    │   │   └── UpdateStatisticsHandlerTests
    │   └── Services/                  # Service unit tests
    │       ├── AuthenticationServiceTests
    │       └── WatchlistServiceTests
    |
    ├── MovieWatchlist.Core.UnitTests/  # Domain layer unit tests
    │   ├── Models/                    # Domain model tests
    │   │   └── WatchlistItemEventTests
    │   ├── Specifications/           # Specification tests
    │   └── ValueObjects/              # Value object tests
    │       └── PasswordValueObjectTests
    |
    ├── MovieWatchlist.Infrastructure.UnitTests/  # Infrastructure layer unit tests
    │   ├── Repositories/              # Repository tests
    │   └── Services/                  # Infrastructure service tests
    │       ├── GenreServiceTests
    │       ├── JwtTokenServiceTests
    │       └── TmdbServiceTests
    |
    ├── MovieWatchlist.Api.IntegrationTests/  # API integration tests
    │   ├── Controllers/                # Controller integration tests
    │   │   ├── AuthControllerTests
    │   │   ├── MoviesControllerTests
    │   │   └── WatchlistControllerTests
    │   └── Integration/               # End-to-end integration tests
    │       └── DomainEventsIntegrationTests
    |
    ├── MovieWatchlist.Persistence.IntegrationTests/  # Persistence integration tests
    │   └── PersistenceIntegrationTests  # Database and repository integration tests
    |
    ├── MovieWatchlist.Infrastructure.IntegrationTests/  # Infrastructure integration tests
    │   └── InfrastructureIntegrationTests  # Infrastructure service integration tests
    |
    └── MovieWatchlist.Tests.Shared/   # Shared test utilities
        ├── Infrastructure/            # Test infrastructure
        │   ├── EnhancedIntegrationTestBase  # Enhanced integration test base
        │   ├── IntegrationTestBase    # Base class for integration tests
        │   ├── TestConstants         # Test constants and fixtures
        │   ├── TestDatabaseSeeder    # Database seeding utilities
        │   ├── TestExtensions        # Test helper extensions
        │   └── WebApplicationFactoryExtensions  # Web app factory helpers
        └── TestDataBuilders/          # Test data builder pattern
            ├── TestDataBuilder       # Main builder factory
            ├── UserBuilder           # User test data builder
            ├── MovieBuilder         # Movie test data builder
            ├── WatchlistItemBuilder # WatchlistItem test data builder
            ├── RefreshTokenBuilder  # RefreshToken test data builder
            ├── PasswordResetTokenBuilder  # PasswordResetToken builder
            ├── MovieDetailsDtoBuilder  # MovieDetailsDto test data builder
            └── TmdbMovieDtoBuilder  # TmdbMovieDto test data builder
    |
    └── movie-watchlist-frontend/       # React Frontend
        ├── src/
        │   ├── features/               # Feature-based organization
        │   │   ├── auth/               # Authentication feature
        │   │   │   ├── api/            # RTK Query API slice
        │   │   │   │   └── authApi.ts # Auth endpoints (injectEndpoints)
        │   │   │   ├── components/     # Auth components
        │   │   │   │   ├── ForgotPasswordForm.tsx
        │   │   │   │   ├── LoginForm.tsx
        │   │   │   │   ├── RegisterForm.tsx
        │   │   │   │   └── ResetPasswordForm.tsx
        │   │   │   ├── contexts/        # Auth context
        │   │   │   │   └── AuthContext.tsx # Thin wrapper around RTK Query
        │   │   │   └── model/          # Auth types & schemas
        │   │   │       ├── auth.types.ts
        │   │   │       └── authSchemas.ts
        │   │   ├── movies/             # Movies feature
        │   │   │   ├── api/            # RTK Query API slice
        │   │   │   │   └── moviesApi.ts # Movie endpoints (injectEndpoints)
        │   │   │   ├── components/     # Movie components
        │   │   │   │   ├── FeaturedMoviesCarousel.tsx
        │   │   │   │   ├── InfiniteMovieList.tsx
        │   │   │   │   ├── MovieCard.tsx
        │   │   │   │   ├── MovieGenres.tsx
        │   │   │   │   ├── MovieList.tsx
        │   │   │   │   ├── MovieMainDetails.tsx
        │   │   │   │   ├── MovieSearch.tsx
        │   │   │   │   ├── PopularMoviesSection.tsx
        │   │   │   │   ├── SearchResults.tsx
        │   │   │   │   ├── TopCastCrew.tsx
        │   │   │   │   └── TrailerSection.tsx
        │   │   │   ├── hooks/          # Movie hooks
        │   │   │   │   ├── useFeaturedMovies.ts
        │   │   │   │   ├── useInfiniteMovies.ts
        │   │   │   │   └── useMovieSearch.ts
        │   │   │   ├── lib/            # Movie utilities
        │   │   │   │   └── tmdbUtils.ts
        │   │   │   └── model/          # Movie types & adapters
        │   │   │       ├── movie.types.ts
        │   │   │       └── movieAdapters.ts
        │   │   └── watchlist/          # Watchlist feature
        │   │       ├── api/            # RTK Query API slice
        │   │       │   └── watchlistApi.ts # Watchlist endpoints (injectEndpoints)
        │   │       ├── components/     # Watchlist components
        │   │       │   ├── AddToWatchlistDialog.tsx
        │   │       │   ├── EditWatchlistItemDialog.tsx
        │   │       │   ├── WatchlistFilters.tsx
        │   │       │   ├── WatchlistGrid.tsx
        │   │       │   ├── WatchlistItemCard.tsx
        │   │       │   └── WatchlistStats.tsx
        │   │       ├── hooks/          # Watchlist hooks
        │   │       │   ├── useAddToWatchlistDialog.ts
        │   │       │   ├── useWatchlistFilters.ts
        │   │       │   ├── useWatchlistOperations.ts
        │   │       │   └── useWatchlistPresence.ts
        │   │       ├── lib/            # Watchlist utilities
        │   │       │   └── watchlistUtils.ts
        │   │       └── model/          # Watchlist types & schemas
        │   │           ├── watchlist.types.ts
        │   │           ├── watchlistSchemas.ts
        │   │           └── watchlistSelectors.ts
        │   ├── shared/                 # Shared code across features
        │   │   ├── api/                # Base API configuration
        │   │   │   ├── baseApi.ts      # Base query with reauth
        │   │   │   └── baseApiSlice.ts # Centralized RTK Query API slice
        │   │   ├── components/         # Shared components
        │   │   │   ├── common/         # Common UI components
        │   │   │   │   ├── ErrorBoundary.tsx
        │   │   │   │   ├── Header.tsx
        │   │   │   │   ├── HeaderAuthButtons.tsx
        │   │   │   │   ├── HeaderLogo.tsx
        │   │   │   │   ├── HeaderSearch.tsx
        │   │   │   │   ├── LoadingSpinner.tsx
        │   │   │   │   ├── LoginRequiredDialog.tsx
        │   │   │   │   ├── QueryErrorBoundary.tsx
        │   │   │   │   ├── SearchDropdown.tsx
        │   │   │   │   ├── SkipLink.tsx
        │   │   │   │   └── skeletons/  # Loading skeletons
        │   │   │   │       ├── MovieCardSkeleton.tsx
        │   │   │   │       ├── MovieDetailsSkeleton.tsx
        │   │   │   │       └── WatchlistItemSkeleton.tsx
        │   │   │   ├── layout/         # Layout components
        │   │   │   │   └── MainLayout.tsx
        │   │   │   └── ui/             # UI state components
        │   │   │       ├── EmptyState.tsx
        │   │   │       ├── ErrorState.tsx
        │   │   │       └── SuccessToast.tsx
        │   │   ├── constants/          # Application constants
        │   │   │   ├── appConstants.ts # API endpoints, tag types, etc.
        │   │   │   ├── formConstants.ts
        │   │   │   └── routeConstants.ts
        │   │   ├── contexts/           # Shared contexts
        │   │   │   └── ErrorContext.tsx # Error notification context
        │   │   ├── hooks/              # Shared hooks
        │   │   │   ├── useForms.ts
        │   │   │   └── useSuccessToast.ts
        │   │   ├── lib/                # Shared utilities
        │   │   │   ├── accessibility.ts
        │   │   │   ├── errorHandler.ts
        │   │   │   ├── formatters.ts
        │   │   │   ├── retryUtils.ts
        │   │   │   ├── test-utils.tsx  # Testing utilities & Redux setup
        │   │   │   └── validationService.ts
        │   │   ├── theme/              # Material-UI theme
        │   │   │   ├── colors.ts
        │   │   │   ├── theme.ts
        │   │   │   └── index.ts
        │   │   └── types/              # Shared types
        │   │       └── error.types.ts
        │   ├── pages/                  # Page components
        │   │   ├── MoviesPage.tsx      # Movie discovery page
        │   │   ├── MovieDetailsPage.tsx # Movie detail page
        │   │   └── WatchlistPage.tsx   # User watchlist page
        │   ├── layouts/                # Layout components
        │   │   └── AuthLayout.tsx     # Authentication layout
        │   ├── routes/                 # Routing configuration
        │   │   └── AppRoutes.tsx      # Route definitions
        │   ├── store/                  # Redux store configuration
        │   │   └── index.ts           # Store setup with baseApiSlice
        │   ├── utils/                  # Helper utilities (legacy)
        │   │   ├── formatters.test.ts
        │   │   └── validationService.test.ts
        │   ├── __tests__/              # Integration tests
        │   │   ├── integration/        # E2E integration tests
        │   │   │   ├── MovieBrowsing.test.tsx
        │   │   │   ├── moviesApi.test.ts
        │   │   │   └── watchlistApi.test.ts
        │   │   ├── fixtures/           # Test fixtures
        │   │   │   ├── authFixtures.ts
        │   │   │   ├── movieFixtures.ts
        │   │   │   └── watchlistFixtures.ts
        │   │   └── TestConstants.ts
        │   ├── App.tsx                 # Main app component
        │   ├── App.test.tsx            # App component tests
        │   ├── index.tsx               # Entry point
        │   └── setupTests.ts           # Test configuration (MSW polyfills)
        └── public/                     # Static assets

## Technologies

### Backend
- [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) - Cross-platform development framework
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web API framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM for data access
- [PostgreSQL](https://www.postgresql.org/) - Relational database
- [JWT](https://jwt.io/) - JSON Web Token authentication
- [Swagger/OpenAPI](https://swagger.io/) - API documentation

### Frontend
- [React](https://reactjs.org/) - Frontend UI library
- [TypeScript](https://www.typescriptlang.org/) - Type-safe JavaScript
- [Material-UI (MUI)](https://mui.com/) - React component library
- [RTK Query](https://redux-toolkit.js.org/rtk-query/overview) - Data fetching & caching
- [Zod](https://zod.dev/) - Schema validation (aligned with backend)
- [MSW (Mock Service Worker)](https://mswjs.io/) - API mocking for tests

## Dependencies

- [xUnit](https://xunit.net/) - Unit testing framework
- [FluentAssertions](https://fluentassertions.com/) - Fluent testing assertions
- [Moq](https://github.com/moq/moq4) - Mocking framework
- [Swagger/OpenAPI](https://swagger.io/) - API documentation

## Tests

### Backend (.NET)
- 303 tests total
- 81% line coverage
- 54.2% branch coverage

### Frontend (React)
- 231 tests total
- 56.27% line coverage
- 47.64% branch coverage