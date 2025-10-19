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
    │   │   └── RateLimitingMiddleware  # Rate limiting
    │   └── Program.cs                  # Application entry point & DI setup
    |
    ├── MovieWatchlist.Application/      # Application Layer (Business Logic)
    │   ├── Services/                   # Business logic implementations
    │   │   ├── AuthenticationService   # User authentication & authorization
    │   │   └── WatchlistService        # Watchlist business rules
    │   └── Validation/                 # Business validation rules
    │       └── InputValidationService  # Input validation logic
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
    │   │   ├── IMovieRepository       # Movie data access interface
    │   │   ├── IWatchlistRepository   # Watchlist data access interface
    │   │   ├── IUserRepository        # User data access interface
    │   │   ├── ITmdbService           # TMDB integration interface
    │   │   ├── IEmailService          # Email service interface
    │   │   ├── IJwtTokenService       # JWT token service interface
    │   │   ├── IPasswordHasher        # Password hashing interface
    │   │   └── IUnitOfWork            # Transaction management interface
    │   ├── Models/                     # Domain entities (business objects)
    │   │   ├── User                    # User entity
    │   │   ├── Movie                   # Movie entity (cached from TMDB)
    │   │   ├── WatchlistItem           # Watchlist entry
    │   │   ├── RefreshToken            # JWT refresh token
    │   │   └── PasswordResetToken      # Password reset token
    │   └── Constants/                  # Domain constants
    │       └── GenreConstants          # Movie genre definitions
    |
    ├── MovieWatchlist.Infrastructure/  # 🔧 Infrastructure Layer (External Concerns)
    │   ├── Configuration/              # Infrastructure configuration
    │   │   └── TmdbSettings           # TMDB API settings
    │   ├── Data/                       # Database context
    │   │   ├── MovieWatchlistDbContext # EF Core context
    │   │   └── MovieWatchlistDbContextFactory # Design-time factory
    │   ├── Migrations/                 # EF Core migrations
    │   ├── Repositories/               # Data access implementations
    │   │   ├── EfRepository           # Generic repository (EF Core)
    │   │   ├── UserRepository         # User data access
    │   │   ├── MovieRepository        # Movie data access
    │   │   └── WatchlistRepository    # Watchlist data access
    │   ├── Services/                   # External service implementations
    │   │   ├── TmdbService            # TMDB API integration
    │   │   ├── EmailService           # Email sending (SMTP)
    │   │   ├── JwtTokenService        # JWT token generation/validation
    │   │   ├── PasswordHasher         # Password hashing (PBKDF2)
    │   │   └── GenreService           # Genre mapping service
    │   └── DTOs/                       # Infrastructure-specific DTOs
    │       └── TmdbMovieDto           # TMDB API response models
    |
    ├── MovieWatchlist.Tests/           # 🧪 Test Suite
    │   ├── Controllers/                # Controller tests
    │   ├── Services/                   # Service unit tests
    │   │   ├── AuthenticationServiceTests
    │   │   ├── WatchlistServiceTests
    │   │   └── InputValidationServiceTests
    │   ├── Infrastructure/             # Repository & integration tests
    │   └── Integration/                # End-to-end tests
    |
    └── movie-watchlist-frontend/       # React Frontend
        ├── src/
        │   ├── components/             # Reusable components
        │   │   ├── auth/               # Authentication components
        │   │   ├── movies/             # Movie display components
        │   │   └── watchlist/          # Watchlist components
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
        │   ├── types/                  # TypeScript types
        │   └── utils/                  # Helper utilities
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