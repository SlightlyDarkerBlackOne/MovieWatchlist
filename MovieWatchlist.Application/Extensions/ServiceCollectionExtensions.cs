using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Application.Behaviors;
using MovieWatchlist.Application.Events.Handlers;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterCommandHandler).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ResultFailureBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddScoped<IDomainEventHandler<MovieWatchedEvent>, UpdateStatisticsHandler>();
        services.AddScoped<IDomainEventHandler<MovieRatedEvent>, UpdateStatisticsHandler>();
        services.AddScoped<IDomainEventHandler<MovieFavoritedEvent>, UpdateStatisticsHandler>();
        services.AddScoped<IDomainEventHandler<StatisticsInvalidatedEvent>, UpdateStatisticsHandler>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IWatchlistService, WatchlistService>();

        return services;
    }
}

