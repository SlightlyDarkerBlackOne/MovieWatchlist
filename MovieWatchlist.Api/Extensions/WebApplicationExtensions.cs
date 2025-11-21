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
            app.UseRateLimiting(maxRequests: 100, timeWindowMinutes: 1);
        }
        else
        {
            app.UseRateLimiting(maxRequests: 10, timeWindowMinutes: 1);
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}

