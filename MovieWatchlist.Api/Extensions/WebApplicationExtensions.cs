using MovieWatchlist.Api.Constants;
using MovieWatchlist.Api.Middleware;

namespace MovieWatchlist.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            
            var urls = app.Configuration["ASPNETCORE_URLS"] ?? app.Urls.FirstOrDefault();
            if (urls != null && urls.Contains("https://", StringComparison.OrdinalIgnoreCase))
            {
                app.UseHttpsRedirection();
            }
        }

        app.UseCors("ReactFrontend");
        app.UseGlobalExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseRateLimiting(
                maxRequests: ConfigurationConstants.RATE_LIMIT_DEVELOPMENT_MAX_REQUESTS,
                timeWindowMinutes: ConfigurationConstants.RATE_LIMIT_TIME_WINDOW_MINUTES);
        }
        else
        {
            app.UseRateLimiting(
                maxRequests: ConfigurationConstants.RATE_LIMIT_PRODUCTION_MAX_REQUESTS,
                timeWindowMinutes: ConfigurationConstants.RATE_LIMIT_TIME_WINDOW_MINUTES);
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}

