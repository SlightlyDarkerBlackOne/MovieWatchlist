# MovieWatchlist

A full-stack movie watchlist application with TMDB integration, built with .NET 8 backend, React frontend, and PostgreSQL database

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
    │   ├── DTOs/                       # API Data Transfer Objects
    │   │   ├── AuthenticationDtos      # Auth request/response models
    │   │   ├── MovieDetailsDto         # Movie details response DTO
    │   │   └── WatchlistDtos          # Watchlist request/response models
    │   ├── Mapping/                     # Mapster mapping profiles
    │   │   ├── AuthMappingProfile      # Auth DTO mappings
    │   │   └── MovieMappingProfile     # Movie DTO mappings
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
    │   │   └── CurrentUserService      # Current user context service
    │   ├── Constants/                  # Configuration constants
    │   │   ├── ConfigurationConstants  # Configuration constants
    │   │   ├── CookieNames             # Cookie name constants
    │   │   ├── EnvironmentVariables    # Environment variable names
    │   │   └── MiddlewareConstants     # Middleware constants
    │   └── Program.cs                  # Application entry point & DI setup
    |
    ├── MovieWatchlist.Application/      # Application Layer (Business Logic)
    │   ├── Commands/                    # CQRS Commands (MediatR)
    │   │   ├── AuthenticationCommands   # Register, Login, Reset Password, etc.
    │   │   └── WatchlistCommands       # Add, Update, Remove watchlist items
    │   ├── Queries/                     # CQRS Queries (MediatR)
    │   │   ├── GetCurrentUserQuery      # Get current authenticated user
    │   │   ├── MovieQueries            # Movie search, details, popular, genre
    │   │   └── WatchlistQueries        # Get watchlist, statistics, filters
    │   ├── Handlers/                    # MediatR Request Handlers
    │   │   ├── Auth/                    # Authentication handlers
    │   │   │   ├── CreateRefreshTokenCommandHandler
    │   │   │   ├── ForgotPasswordCommandHandler
    │   │   │   ├── GetCurrentUserQueryHandler
    │   │   │   ├── LoginCommandHandler
    │   │   │   ├── LogoutCommandHandler
    │   │   │   ├── RefreshTokenCommandHandler
    │   │   │   ├── RegisterCommandHandler
    │   │   │   ├── ResetPasswordCommandHandler
    │   │   │   └── ValidateTokenCommandHandler
    │   │   ├── Movies/                  # Movie query handlers
    │   │   │   ├── GetMovieDetailsByTmdbIdQueryHandler # With caching logic
    │   │   │   ├── GetMovieDetailsQueryHandler
    │   │   │   ├── GetMoviesByGenreQueryHandler
    │   │   │   ├── GetPopularMoviesQueryHandler
    │   │   │   └── SearchMoviesQueryHandler
    │   │   └── Watchlist/               # Watchlist handlers
    │   │       ├── AddToWatchlistCommandHandler
    │   │       ├── GetFavoriteMoviesQueryHandler
    │   │       ├── GetRecommendedMoviesQueryHandler
    │   │       ├── GetUserStatisticsQueryHandler
    │   │       ├── GetUserWatchlistQueryHandler
    │   │       ├── GetWatchlistByGenreQueryHandler
    │   │       ├── GetWatchlistByRatingRangeQueryHandler
    │   │       ├── GetWatchlistByStatusQueryHandler
    │   │       ├── GetWatchlistByYearRangeQueryHandler
    │   │       ├── GetWatchlistItemByIdQueryHandler
    │   │       ├── RemoveFromWatchlistCommandHandler
    │   │       └── UpdateWatchlistItemCommandHandler
    │   ├── Behaviors/                   # MediatR pipeline behaviors
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
    │   │   └── IDomainEventHandler    # Domain event handler
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
    │   │   └── ApiException           # Custom exception hierarchy
    │   └── Constants/                  # Domain constants
    │       ├── ErrorMessages          # Centralized error messages
    │       ├── GenreConstants         # Movie genre definitions
    │       └── ValidationConstants    # Validation rules
    |
    ├── MovieWatchlist.Infrastructure/  # 🔧 Infrastructure Layer (External Concerns)
    │   ├── Configuration/              # Infrastructure configuration
    │   │   └── TmdbSettings           # TMDB API settings
    │   ├── Data/                       # Database context
    │   │   ├── MovieWatchlistDbContext # EF Core context
    │   │   └── MovieWatchlistDbContextFactory # Design-time factory
    │   ├── Migrations/                 # EF Core migrations
    │   │   ├── InitialCreate          # Initial database schema
    │   │   ├── AddPasswordResetToken  # Password reset support
    │   │   ├── AddCreditsAndVideosToMovie # Movie credits/videos caching
    │   │   ├── AddValueObjectsSupport # Value object support
    │   │   └── AddUserStatisticsAndNullableRating # Statistics caching
    │   ├── Repositories/               # Data access implementations
    │   │   ├── EfRepository           # Generic repository (EF Core) + UnitOfWork
    │   │   ├── UserRepository         # User data access
    │   │   ├── MovieRepository        # Movie data access
    │   │   ├── WatchlistRepository    # Watchlist data access
    │   │   ├── RefreshTokenRepository # Refresh token repository
    │   │   ├── PasswordResetTokenRepository # Password reset repository
    │   │   └── InMemoryRepository     # In-memory repository for testing
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
    ├── MovieWatchlist.Tests/           # Test Suite
    │   ├── Controllers/                # Controller tests
    │   │   ├── AuthControllerTests     # Auth controller integration tests
    │   │   ├── MoviesControllerTests   # Movies controller unit tests
    │   │   └── WatchlistControllerTests # Watchlist controller tests
    │   ├── Application/                # Application layer tests
    │   │   └── Events/                 # Event handler tests
    │   │       ├── DomainEventDispatcherTests
    │   │       └── Handlers/
    │   │           └── LogActivityHandlerTests
    │   ├── Core/                       # Domain layer tests
    │   │   ├── Models/
    │   │   │   └── WatchlistItemEventTests
    │   │   └── ValueObjects/
    │   │       └── PasswordValueObjectTests
    │   ├── Infrastructure/             # Infrastructure test utilities
    │   │   ├── EnhancedIntegrationTestBase # Enhanced integration test base
    │   │   ├── IntegrationTestBase     # Base class for integration tests
    │   │   ├── TestConstants          # Test constants and fixtures
    │   │   ├── TestDatabaseSeeder     # Database seeding utilities
    │   │   ├── TestDataBuilder        # Test data builder pattern
    │   │   ├── TestExtensions         # Test helper extensions
    │   │   ├── UnitTestBase           # Base class for unit tests
    │   │   └── WebApplicationFactoryExtensions # Web app factory helpers
    │   ├── Integration/                # End-to-end tests
    │   │   ├── DomainEventsIntegrationTests
    │   │   └── InfrastructureIntegrationTests
    │   └── Services/                   # Service unit tests
    │       ├── AuthenticationServiceTests
    │       ├── GenreServiceTests
    │       ├── JwtTokenServiceTests
    │       ├── TmdbServiceTests
    │       └── WatchlistServiceTests
    |
    └── movie-watchlist-frontend/       # React Frontend
        ├── src/
        │   ├── components/             # Reusable components
        │   │   ├── auth/               # Authentication components
        │   │   │   ├── ForgotPasswordForm   # Forgot password form
        │   │   │   ├── LoginForm           # Login form
        │   │   │   ├── RegisterForm         # Registration form
        │   │   │   └── ResetPasswordForm    # Password reset form
        │   │   ├── common/             # Common components
        │   │   │   ├── ErrorBoundary   # Error boundary wrapper
        │   │   │   ├── Header           # Main navigation header
        │   │   │   ├── HeaderAuthButtons # Auth buttons in header
        │   │   │   ├── HeaderLogo       # Logo component
        │   │   │   ├── HeaderSearch     # Search in header
        │   │   │   ├── LoadingSpinner   # Loading spinner
        │   │   │   ├── LoginRequiredDialog   # Login prompt dialog
        │   │   │   ├── QueryErrorBoundary    # RTK Query error boundary
        │   │   │   ├── SearchDropdown   # Search autocomplete
        │   │   │   ├── SkipLink         # Accessibility skip link
        │   │   │   └── skeletons/       # Loading skeleton components
        │   │   │       ├── MovieCardSkeleton
        │   │   │       ├── MovieDetailsSkeleton
        │   │   │       └── WatchlistItemSkeleton
        │   │   ├── dialogs/            # Modal dialogs
        │   │   │   ├── AddToWatchlistDialog    # Add to watchlist dialog
        │   │   │   └── EditWatchlistItemDialog  # Edit watchlist item dialog
        │   │   ├── layout/             # Layout components
        │   │   │   └── MainLayout     # Main app layout
        │   │   ├── movies/             # Movie display components
        │   │   │   ├── FeaturedMoviesCarousel  # Hero carousel
        │   │   │   ├── InfiniteMovieList      # Infinite scroll list
        │   │   │   ├── MovieCard               # Movie card component
        │   │   │   ├── MovieGenres             # Genre chips
        │   │   │   ├── MovieList                # Movie list component
        │   │   │   ├── MovieMainDetails        # Movie detail header
        │   │   │   ├── MovieSearch             # Search input
        │   │   │   └── TopCastCrew             # Cast & crew display
        │   │   ├── pages/              # Page-level components
        │   │   │   ├── PopularMoviesSection    # Popular movies section
        │   │   │   ├── SearchResults           # Search results page
        │   │   │   ├── TrailerSection          # Video trailer player
        │   │   │   ├── WatchlistFilters         # Watchlist filters
        │   │   │   └── WatchlistStats          # Watchlist statistics
        │   │   ├── ui/                 # UI state components
        │   │   │   ├── EmptyState      # Empty state display
        │   │   │   ├── ErrorState      # Error state display
        │   │   │   └── SuccessToast    # Success toast notification
        │   │   └── watchlist/          # Watchlist components
        │   │       ├── WatchlistGrid    # Watchlist grid layout
        │   │       ├── WatchlistItemCard       # Watchlist item card
        │   │       ├── WatchlistFilters        # Filter controls
        │   │       └── WatchlistStats           # Statistics display
        │   ├── contexts/               # React contexts
        │   │   ├── AuthContext         # Authentication state
        │   │   └── WatchlistContext    # Watchlist state (Set-based O(1) lookups)
        │   ├── hooks/                  # Custom React hooks
        │   │   ├── useAddToWatchlistDialog # Watchlist dialog hook
        │   │   ├── useFeaturedMovies   # Featured movies hook
        │   │   ├── useForms            # Form handling hooks
        │   │   ├── useInfiniteMovies   # Infinite scroll hook
        │   │   ├── useMovieSearch      # Movie search hook
        │   │   ├── useSuccessToast     # Success toast hook
        │   │   ├── useWatchlistFilters # Watchlist filtering logic
        │   │   ├── useWatchlistOperations  # RTK Query hook exports
        │   │   └── useWatchlistPresence # Check if movie in watchlist
        │   ├── pages/                  # Page components
        │   │   ├── MoviesPage          # Movie discovery page
        │   │   ├── MovieDetailsPage    # Movie detail page
        │   │   └── WatchlistPage      # User watchlist page
        │   ├── store/                  # Redux store (RTK Query)
        │   │   └── api/               # RTK Query APIs
        │   │       ├── moviesApi       # Movie endpoints
        │   │       └── watchlistApi    # Watchlist endpoints
        │   ├── services/               # Service layer
        │   │   ├── api               # Axios instance with interceptors
        │   │   ├── authService        # Authentication service
        │   │   ├── movieService      # Movie utilities (TMDB helpers)
        │   │   └── watchlistService  # Watchlist utilities (status helpers)
        │   ├── constants/              # Application constants
        │   │   └── constants          # Route and API endpoint constants
        │   ├── theme/                  # Material-UI theme
        │   │   ├── colors            # Color palette
        │   │   ├── theme             # Theme configuration
        │   │   └── index             # Theme exports
        │   ├── types/                  # TypeScript types
        │   │   ├── auth.types        # Auth types
        │   │   ├── movie.types       # Movie types
        │   │   ├── watchlist.types   # Watchlist types
        │   │   └── error.types      # Error types
        │   ├── utils/                  # Helper utilities
        │   │   ├── cacheService      # Browser cache utilities
        │   │   ├── errorHandler      # Centralized error extraction
        │   │   ├── formatters        # Data formatting utilities
        │   │   ├── test-utils        # Testing utilities & Redux setup
        │   │   ├── tmdbTransformers  # TMDB data transformers
        │   │   └── validationService # Frontend validation service
        │   ├── validation/             # Zod schemas
        │   │   └── schemas           # Validation schemas (aligned with backend)
        │   ├── routes/                 # Routing configuration
        │   │   └── AppRoutes         # Route definitions
        │   ├── layouts/                # Layout components
        │   │   └── AuthLayout       # Authentication layout
        │   ├── __tests__/             # Integration tests
        │   │   ├── integration/     # E2E integration tests
        │   │   │   ├── moviesApi.test.ts
        │   │   │   └── watchlistApi.test.ts
        │   │   └── fixtures/        # Test fixtures
        │   ├── App.tsx                # Main app component
        │   ├── index.tsx              # Entry point
        │   └── setupTests.ts         # Test configuration (MSW polyfills)
        └── public/                    # Static assets

## Technologies

### Backend
- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) - Cross-platform development framework
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
- 230 tests total
- 77.55% line coverage
- 52.38% branch coverage

### Frontend (React)