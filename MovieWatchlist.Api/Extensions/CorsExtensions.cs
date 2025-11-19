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
                if (environment.IsDevelopment())
                {
                    policy.WithOrigins(ConfigurationConstants.DEFAULT_FRONTEND_URL)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
                else
                {
                    policy.WithOrigins(ConfigurationConstants.PRODUCTION_FRONTEND_URL)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });

        return services;
    }
}

