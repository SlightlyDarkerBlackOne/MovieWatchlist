using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Infrastructure.Configuration;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Api.Middleware;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Application.Events;
using MovieWatchlist.Application.Events.Handlers;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Infrastructure.Repositories;
using MovieWatchlist.Infrastructure.Services;

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
    c.SwaggerDoc("v1", new() { Title = "MovieWatchlist API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
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
builder.Services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieRatedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieFavoritedEvent>, LogActivityHandler>();
builder.Services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, UpdateStatisticsHandler>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();

// Register Infrastructure services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddHttpClient<ITmdbService, TmdbService>();

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

app.UseTransactionPerRequest();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
