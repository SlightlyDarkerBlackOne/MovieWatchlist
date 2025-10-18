# MovieWatchlist

A full-stack movie watchlist application with TMDB integration, built with .NET 8 backend, React frontend, and PostgreSQL database

## Description

A comprehensive movie management platform that allows users to discover, track, and organize their movie watchlists. Features include movie search, personal watchlist management, and genre-based recommendations.

## Structure

    MovieWatchlist # Solution
    |
    ├── MovieWatchlist.Api/              # Web API Layer
    │   ├── Controllers/                 # API Controllers
    │   │   ├── AuthController          # Authentication endpoints
    │   │   ├── MoviesController        # Movie search and details
    │   │   └── WatchlistController     # Watchlist management
    │   ├── Middleware/                 # Custom middleware
    │   │   ├── GlobalExceptionMiddleware # Error handling
    │   │   └── RateLimitingMiddleware  # Rate limiting
    │   └── Program.cs                  # Application entry point
    |
    ├── MovieWatchlist.Core/            # Business Logic Layer
    │   ├── Configuration/              # App settings
    │   │   ├── JwtSettings            # JWT configuration
    │   │   └── TmdbSettings           # TMDB API settings
    │   ├── DTOs/                      # Data Transfer Objects
    │   │   ├── AuthenticationDtos     # Auth request/response
    │   │   ├── TmdbMovieDto          # TMDB movie data
    │   │   └── UpdateWatchlistItemDto # Watchlist updates
    │   ├── Interfaces/                # Service contracts
    │   │   ├── IAuthenticationService # Auth service interface
    │   │   ├── IMovieRepository       # Movie data access
    │   │   ├── IWatchlistService      # Watchlist business logic
    │   │   └── ITmdbService          # TMDB integration
    │   ├── Models/                    # Domain models
    │   │   ├── User                   # User entity
    │   │   ├── Movie                  # Movie entity
    │   │   ├── WatchlistItem          # Watchlist entry
    │   │   └── RefreshToken           # JWT refresh token
    │   └── Validation/                # Input validation
    │       └── InputValidationService # Validation logic
    |
    ├── MovieWatchlist.Infrastructure/ # Data Access Layer
    │   ├── Data/                      # Database context
    │   │   └── MovieWatchlistDbContext # EF Core context
    │   ├── Repositories/              # Data repositories
    │   │   ├── UserRepository         # User data access
    │   │   ├── MovieRepository        # Movie data access
    │   │   └── WatchlistRepository    # Watchlist data access
    │   └── Services/                  # External services
    │       ├── TmdbService            # TMDB API integration
    │       ├── AuthenticationService  # Auth implementation
    │       └── WatchlistService       # Watchlist business logic
    |
    ├── MovieWatchlist.Tests/          # Test Suite
    │   ├── Controllers/               # Controller tests
    │   ├── Services/                  # Service unit tests
    │   ├── Infrastructure/            # Integration tests
    │   └── Integration/               # End-to-end tests
    |
    └── movie-watchlist-frontend/      # React Frontend
        ├── src/
        │   ├── components/            # Reusable components
        │   │   ├── auth/              # Authentication components
        │   │   ├── movies/            # Movie display components
        │   │   └── watchlist/         # Watchlist components
        │   ├── pages/                 # Page components
        │   │   ├── MoviesPage         # Movie discovery
        │   │   ├── WatchlistPage      # User watchlist
        │   │   └── MovieDetailsPage   # Movie details
        │   ├── contexts/              # React contexts
        │   │   ├── AuthContext        # Authentication state
        │   │   └── WatchlistContext   # Watchlist state
        │   ├── services/              # API services
        │   │   ├── authService        # Auth API calls
        │   │   ├── movieService       # Movie API calls
        │   │   └── watchlistService   # Watchlist API calls
        │   ├── types/                 # TypeScript types
        │   └── utils/                 # Helper utilities
        └── public/                    # Static assets

## Technologies

- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) - Cross-platform development framework
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web API framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM for data access
- [PostgreSQL](https://www.postgresql.org/) - Relational database
- [React](https://reactjs.org/) - Frontend UI library
- [TypeScript](https://www.typescriptlang.org/) - Type-safe JavaScript
- [Material-UI](https://mui.com/) - React component library
- [JWT](https://jwt.io/) - JSON Web Token authentication

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