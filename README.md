# MovieWatchlist

A full-stack movie watchlist application with TMDB integration, built with .NET 8 backend, React frontend, and PostgreSQL database

## Description

A comprehensive movie management platform that allows users to discover, track, and organize their movie watchlists. Features include movie search, personal watchlist management, and genre-based recommendations.

## Structure

    MovieWatchlist # Solution
    |
    ├── MovieWatchlist.Api/              # Presentation Layer
    │   ├── Controllers/                 # API Controllers (HTTP endpoints)
    │   │   ├── AuthController          # Authentication endpoints
    │   │   ├── MoviesController        # Movie search and details
    │   │   └── WatchlistController     # Watchlist management
    │   ├── DTOs/                       # API Data Transfer Objects
    │   │   ├── AuthenticationDtos      # Auth request/response models
    │   │   └── WatchlistDtos          # Watchlist request/response models
    │   ├── Middleware/                 # Custom middleware
    │   │   ├── GlobalExceptionMiddleware # Error handling
    │   │   ├── RateLimitingMiddleware  # Rate limiting
    │   │   └── TransactionMiddleware   # Unit of Work per request
    │   ├── Constants/                  # Configuration constants
    │   │   ├── ConfigurationConstants  # Configuration constants
    │   │   ├── EnvironmentVariables    # Environment variable names
    │   │   └── MiddlewareConstants     # Middleware constants
    │   └── Program.cs                  # Application entry point & DI setup
    |
    ├── MovieWatchlist.Application/      # Application Layer (Business Logic)
    │   ├── Services/                   # Business logic implementations
    │   │   ├── AuthenticationService   # User authentication & authorization
    │   │   └── WatchlistService        # Watchlist business rules
    │   ├── Validation/                 # Business validation rules
    │   │   └── InputValidationService  # Input validation logic
    │   └── Events/                     # Domain event handling
    │       ├── DomainEventDispatcher   # Event dispatcher
    │       └── Handlers/               # Event handlers
    │           ├── LogActivityHandler  # Log domain events
    │           └── UpdateStatisticsHandler # Update statistics
    |
    ├── MovieWatchlist.Core/            # Domain Layer (Core Business)
    │   ├── Commands/                   # Write operations (CQRS pattern)
    │   │   ├── AuthenticationCommands  # Register, Login, Reset Password
    │   │   └── WatchlistCommands      # Add, Update, Remove items
    │   ├── Queries/                    # Read operations (CQRS pattern)
    │   │   └── WatchlistQueries       # Get watchlist, statistics, filters
    │   ├── Configuration/              # Configuration models
    │   │   └── JwtSettings            # JWT configuration
    │   ├── Interfaces/                 # Service contracts (abstractions)
    │   │   ├── IAuthenticationService  # Auth service interface
    │   │   ├── IWatchlistService      # Watchlist service interface
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
    │   │   ├── IDomainEventDispatcher # Domain event dispatcher
    │   │   └── IDomainEventHandler    # Domain event handler
    │   ├── Models/                     # Domain entities (business objects)
    │   │   ├── Entity                 # Base entity with domain events
    │   │   ├── User                   # User entity
    │   │   ├── Movie                  # Movie entity (cached from TMDB)
    │   │   ├── WatchlistItem          # Watchlist entry
    │   │   ├── RefreshToken           # JWT refresh token
    │   │   └── PasswordResetToken     # Password reset token
    │   ├── ValueObjects/               # Domain value objects
    │   │   ├── Email                  # Email value object
    │   │   ├── Username               # Username value object
    │   │   └── Rating                 # Rating value object
    │   ├── Events/                     # Domain events
    │   │   ├── IDomainEvent           # Domain event interface
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
    │   │   └── PasswordHasher         # Password hashing (PBKDF2)
    │   └── DTOs/                       # Infrastructure-specific DTOs
    │       └── TmdbMovieDto           # TMDB API response models
    |
    ├── MovieWatchlist.Tests/           # Test Suite
    │   ├── Application/                # Application layer tests
    │   │   ├── AuthenticationServiceTests
    │   │   └── WatchlistServiceTests
    │   ├── Controllers/                # Controller tests
    │   │   └── AuthControllerTests
    │   ├── Core/                       # Domain layer tests
    │   │   └── Models/
    │   │       └── WatchlistItemEventTests
    │   ├── Infrastructure/             # Infrastructure tests
    │   │   ├── Repositories/          # Repository tests
    │   │   └── Services/              # Service tests
    │   ├── Integration/                # End-to-end tests
    │   │   ├── AuthFlowTests
    │   │   └── WatchlistFlowTests
    │   └── Services/                   # Service unit tests
    │       ├── AuthenticationServiceTests
    │       ├── WatchlistServiceTests
    │       ├── InputValidationServiceTests
    │       └── DomainEventDispatcherTests
    |
    └── movie-watchlist-frontend/       # React Frontend
        ├── src/
        │   ├── components/             # Reusable components
        │   │   ├── auth/               # Authentication components
        │   │   ├── movies/             # Movie display components
        │   │   ├── watchlist/          # Watchlist components
        │   │   └── common/             # Common components
        │   ├── pages/                  # Page components
        │   │   ├── MoviesPage          # Movie discovery
        │   │   ├── WatchlistPage       # User watchlist
        │   │   └── MovieDetailsPage    # Movie details
        │   ├── contexts/               # React contexts
        │   │   ├── AuthContext         # Authentication state
        │   │   └── WatchlistContext    # Watchlist state
        │   ├── services/               # API services
        │   │   ├── authService         # Auth API calls
        │   │   ├── movieService        # Movie API calls
        │   │   └── watchlistService    # Watchlist API calls
        │   ├── constants/              # Application constants
        │   │   ├── formConstants       # Form validation constants
        │   │   └── routeConstants      # Route definitions
        │   ├── theme/                  # Material-UI theme
        │   │   ├── colors              # Color palette
        │   │   ├── theme               # Theme configuration
        │   │   └── index               # Theme exports
        │   ├── layouts/                # Layout components
        │   │   └── AuthLayout          # Authentication layout
        │   ├── routes/                 # Routing configuration
        │   │   └── AppRoutes           # Route definitions
        │   ├── types/                  # TypeScript types
        │   │   ├── auth.types          # Auth types
        │   │   ├── movie.types         # Movie types
        │   │   ├── watchlist.types     # Watchlist types
        │   │   └── error.types         # Error types
        │   ├── utils/                  # Helper utilities
        │   │   ├── cacheService        # Caching utilities
        │   │   ├── formatters          # Formatting utilities
        │   │   ├── tmdbTransformers    # TMDB data transformers
        │   │   └── validationService   # Validation utilities
        │   ├── App.tsx                 # Main app component
        │   ├── index.tsx               # Entry point
        │   └── setupTests.ts           # Test configuration
        └── public/                     # Static assets

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
- [Material-UI](https://mui.com/) - React component library

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
- 258 tests total
- 67.68% line coverage
- 55.84% branch coverage