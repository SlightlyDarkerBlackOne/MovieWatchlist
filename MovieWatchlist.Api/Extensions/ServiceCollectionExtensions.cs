using Mapster;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Api.Helpers;
using MovieWatchlist.Api.Mapping;
using MovieWatchlist.Api.Options;
using MovieWatchlist.Api.Services;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Infrastructure.Configuration;

namespace MovieWatchlist.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenExtractor, TokenExtractor>();
        services.AddScoped<IAuthCookieManager, AuthCookieManager>();
        services.AddScoped<IAuthCookieService, AuthCookieService>();

        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Scan(typeof(AuthMappingProfile).Assembly);
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);

        return services;
    }

    public static IServiceCollection ConfigureSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TmdbSettings>(options =>
        {
            options.ApiKey = configuration.GetRequiredValue(
                EnvironmentVariables.TMDB_API_KEY,
                ConfigurationConstants.TMDB_SETTINGS_API_KEY);
            options.BaseUrl = configuration.GetOptionalValue(
                EnvironmentVariables.TMDB_BASE_URL,
                ConfigurationConstants.TMDB_SETTINGS_BASE_URL,
                ConfigurationConstants.DEFAULT_TMDB_BASE_URL);
            options.ImageBaseUrl = configuration.GetOptionalValue(
                EnvironmentVariables.TMDB_IMAGE_BASE_URL,
                ConfigurationConstants.TMDB_SETTINGS_IMAGE_BASE_URL,
                ConfigurationConstants.DEFAULT_TMDB_IMAGE_BASE_URL);
        });

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = configuration.GetRequiredValue(
                EnvironmentVariables.JWT_SECRET_KEY,
                ConfigurationConstants.JWT_SETTINGS_SECRET_KEY);
            options.Issuer = configuration.GetRequiredValue(
                EnvironmentVariables.JWT_ISSUER,
                ConfigurationConstants.JWT_SETTINGS_ISSUER);
            options.Audience = configuration.GetRequiredValue(
                EnvironmentVariables.JWT_AUDIENCE,
                ConfigurationConstants.JWT_SETTINGS_AUDIENCE);
            options.ExpirationMinutes = configuration.GetRequiredInt(
                EnvironmentVariables.JWT_EXPIRATION_MINUTES,
                ConfigurationConstants.JWT_SETTINGS_EXPIRATION_MINUTES);
            options.RefreshTokenExpirationDays = configuration.GetRequiredInt(
                EnvironmentVariables.JWT_REFRESH_DAYS,
                ConfigurationConstants.JWT_SETTINGS_REFRESH_TOKEN_EXPIRATION_DAYS);
        });

        services.Configure<AuthCookieOptions>(
            configuration.GetSection(ConfigurationConstants.AUTH_COOKIE_SETTINGS));

        return services;
    }
}

