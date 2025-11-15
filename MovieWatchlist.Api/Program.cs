using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Infrastructure.Configuration;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Api.Middleware;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Application.Events.Handlers;
using MovieWatchlist.Infrastructure.Events;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Persistence.Repositories;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Api.Services;
using MovieWatchlist.Api.Options;
using MovieWatchlist.Api.Helpers;
using Mapster;
using MovieWatchlist.Api.Mapping;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHealthChecks();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MovieWatchlist API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            new[] { "Bearer" }
        }
    });
});

// Configure settings with environment variable support
builder.Services.Configure<TmdbSettings>(options =>
{
    options.ApiKey = builder.Configuration[EnvironmentVariables.TMDB_API_KEY] ?? builder.Configuration[ConfigurationConstants.TMDB_SETTINGS_API_KEY]!;
    options.BaseUrl = builder.Configuration[EnvironmentVariables.TMDB_BASE_URL] ?? builder.Configuration[ConfigurationConstants.TMDB_SETTINGS_BASE_URL] ?? ConfigurationConstants.DEFAULT_TMDB_BASE_URL;
    options.ImageBaseUrl = builder.Configuration[EnvironmentVariables.TMDB_IMAGE_BASE_URL] ?? builder.Configuration[ConfigurationConstants.TMDB_SETTINGS_IMAGE_BASE_URL] ?? ConfigurationConstants.DEFAULT_TMDB_IMAGE_BASE_URL;
});

builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = builder.Configuration[EnvironmentVariables.JWT_SECRET_KEY] ?? builder.Configuration[ConfigurationConstants.JWT_SETTINGS_SECRET_KEY]!;
    options.Issuer = builder.Configuration[EnvironmentVariables.JWT_ISSUER] ?? builder.Configuration[ConfigurationConstants.JWT_SETTINGS_ISSUER]!;
    options.Audience = builder.Configuration[EnvironmentVariables.JWT_AUDIENCE] ?? builder.Configuration[ConfigurationConstants.JWT_SETTINGS_AUDIENCE]!;
    options.ExpirationMinutes = int.Parse(builder.Configuration[EnvironmentVariables.JWT_EXPIRATION_MINUTES] ?? builder.Configuration[ConfigurationConstants.JWT_SETTINGS_EXPIRATION_MINUTES]!);
    options.RefreshTokenExpirationDays = int.Parse(builder.Configuration[EnvironmentVariables.JWT_REFRESH_DAYS] ?? builder.Configuration[ConfigurationConstants.JWT_SETTINGS_REFRESH_TOKEN_EXPIRATION_DAYS]!);
});

builder.Services.Configure<AuthCookieOptions>(
    builder.Configuration.GetSection(ConfigurationConstants.AUTH_COOKIE_SETTINGS));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        
        // Override with environment variables if present
        jwtSettings.SecretKey = builder.Configuration[EnvironmentVariables.JWT_SECRET_KEY] ?? jwtSettings.SecretKey;
        jwtSettings.Issuer = builder.Configuration[EnvironmentVariables.JWT_ISSUER] ?? jwtSettings.Issuer;
        jwtSettings.Audience = builder.Configuration[EnvironmentVariables.JWT_AUDIENCE] ?? jwtSettings.Audience;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Read JWT from httpOnly cookies when Authorization header is not present
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    var tokenFromCookie = context.Request.Cookies[MovieWatchlist.Core.Constants.CookieNames.AccessToken];
                    if (!string.IsNullOrEmpty(tokenFromCookie))
                    {
                        context.Token = tokenFromCookie;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure Entity Framework
builder.Services.AddDbContext<MovieWatchlistDbContext>(options =>
    options.UseNpgsql(builder.Configuration[EnvironmentVariables.DATABASE_CONNECTION_STRING] ?? builder.Configuration.GetConnectionString(ConfigurationConstants.DEFAULT_CONNECTION_STRING)));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

// Register Domain Events infrastructure
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Register Domain Event Handlers
// Logging handlers (Infrastructure)
builder.Services.AddScoped<IDomainEventHandler<MovieAddedToWatchlistEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieRemovedFromWatchlistEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieRatedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieFavoritedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<StatisticsInvalidatedEvent>, LogActivityHandler>();

// Business logic handlers (Application)
builder.Services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, UpdateStatisticsHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieRatedEvent>, UpdateStatisticsHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieFavoritedEvent>, UpdateStatisticsHandler>();
builder.Services.AddScoped<IDomainEventHandler<StatisticsInvalidatedEvent>, UpdateStatisticsHandler>();

// Authentication event handlers (Infrastructure - logging only)
builder.Services.AddScoped<IDomainEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<UserLoggedInEvent>, UserLoggedInEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<RefreshTokenCreatedEvent>, RefreshTokenCreatedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<UserPasswordChangedEvent>, UserPasswordChangedEventHandler>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MovieWatchlist.Application.Features.Auth.Commands.Register.RegisterCommandHandler).Assembly);
    cfg.AddOpenBehavior(typeof(MovieWatchlist.Infrastructure.Behaviors.LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(MovieWatchlist.Application.Behaviors.ResultFailureBehavior<,>));
    cfg.AddOpenBehavior(typeof(MovieWatchlist.Application.Behaviors.TransactionBehavior<,>));
});

builder.Services.AddScoped<IAuthCookieManager, AuthCookieManager>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();

// Register Application services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();

// Register Infrastructure services
builder.Services.AddScoped<IRetryPolicyService, RetryPolicyService>();

// HttpContext + Current User
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenExtractor, TokenExtractor>();

// Register Infrastructure services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddHttpClient<ITmdbService, TmdbService>();

builder.Services.AddMapster();
TypeAdapterConfig.GlobalSettings.Scan(typeof(AuthMappingProfile).Assembly);
TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieWatchlist.Application.Features.Movies.Common.MovieMappingProfile).Assembly);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(ConfigurationConstants.DEFAULT_FRONTEND_URL)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Allow cookies/auth headers
        }
        else
        {
            // Production: Allow specific domains
            policy.WithOrigins(ConfigurationConstants.PRODUCTION_FRONTEND_URL)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in development
// Render handles HTTPS at the load balancer level
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add CORS middleware (must be before authentication)
app.UseCors("ReactFrontend");

// Add middleware in correct order
app.UseGlobalExceptionHandler();

// Rate limiting - more lenient in development
if (app.Environment.IsDevelopment())
{
    app.UseRateLimiting(maxRequests: 100, timeWindowMinutes: 1); // 100 requests per minute for development
}
else
{
    app.UseRateLimiting(maxRequests: 10, timeWindowMinutes: 1); // 10 requests per minute for production
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }

