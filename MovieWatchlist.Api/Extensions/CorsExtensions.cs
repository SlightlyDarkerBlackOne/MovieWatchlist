using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovieWatchlist.Api.Constants;

namespace MovieWatchlist.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("ReactFrontend", policy =>
            {
                var origins = new List<string> { ConfigurationConstants.PRODUCTION_FRONTEND_URL };
                
                if (environment.IsDevelopment())
                {
                    origins.Add(ConfigurationConstants.DEFAULT_FRONTEND_URL);
                }

                policy.WithOrigins(origins.ToArray())
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromHours(ConfigurationConstants.CORS_PREFLIGHT_CACHE_HOURS));
            });
        });

        return services;
    }
}

