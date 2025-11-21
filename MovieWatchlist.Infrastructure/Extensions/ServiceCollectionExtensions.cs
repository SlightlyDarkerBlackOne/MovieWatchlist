using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Infrastructure.Events;
using MovieWatchlist.Infrastructure.Services;

namespace MovieWatchlist.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IDomainEventHandler<MovieAddedToWatchlistEvent>, LogActivityHandler>();
        services.AddScoped<IDomainEventHandler<MovieRemovedFromWatchlistEvent>, LogActivityHandler>();
        services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, LogActivityHandler>();
        services.AddScoped<IDomainEventHandler<MovieRatedEvent>, LogActivityHandler>();
        services.AddScoped<IDomainEventHandler<MovieFavoritedEvent>, LogActivityHandler>();
        services.AddScoped<IDomainEventHandler<StatisticsInvalidatedEvent>, LogActivityHandler>();

        services.AddScoped<IDomainEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IDomainEventHandler<UserLoggedInEvent>, UserLoggedInEventHandler>();
        services.AddScoped<IDomainEventHandler<RefreshTokenCreatedEvent>, RefreshTokenCreatedEventHandler>();
        services.AddScoped<IDomainEventHandler<UserPasswordChangedEvent>, UserPasswordChangedEventHandler>();

        services.AddScoped<IRetryPolicyService, RetryPolicyService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddHttpClient<ITmdbService, TmdbService>();

        return services;
    }
}

