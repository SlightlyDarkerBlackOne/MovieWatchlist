using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Events;

public class LogActivityHandler :
    IDomainEventHandler<MovieAddedToWatchlistEvent>,
    IDomainEventHandler<MovieRemovedFromWatchlistEvent>,
    IDomainEventHandler<MovieWatchedEvent>,
    IDomainEventHandler<MovieRatedEvent>,
    IDomainEventHandler<MovieFavoritedEvent>,
    IDomainEventHandler<StatisticsInvalidatedEvent>
{
    private readonly ILogger<LogActivityHandler> _logger;
    private readonly IMovieRepository _movieRepository;
    
    public LogActivityHandler(ILogger<LogActivityHandler> logger, IMovieRepository movieRepository)
    {
        _logger = logger;
        _movieRepository = movieRepository;
    }
    
    public async Task HandleAsync(MovieAddedToWatchlistEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByTmdbIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        _logger.LogInformation(
            "User {UserId} added movie '{MovieTitle}' ({MovieId}) to watchlist with status {Status}",
            domainEvent.UserId,
            movieTitle,
            domainEvent.MovieId,
            domainEvent.InitialStatus
        );
    }
    
    public async Task HandleAsync(MovieRemovedFromWatchlistEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByTmdbIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        _logger.LogInformation(
            "User {UserId} removed movie '{MovieTitle}' ({MovieId}) from watchlist. Final status: {Status}",
            domainEvent.UserId,
            movieTitle,
            domainEvent.MovieId,
            domainEvent.FinalStatus
        );
    }
    
    public async Task HandleAsync(MovieWatchedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByTmdbIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        _logger.LogInformation(
            "User {UserId} watched movie '{MovieTitle}' ({MovieId}) at {WatchedDate}",
            domainEvent.UserId,
            movieTitle,
            domainEvent.MovieId,
            domainEvent.WatchedDate
        );
    }
    
    public async Task HandleAsync(MovieRatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByTmdbIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        _logger.LogInformation(
            "User {UserId} rated movie '{MovieTitle}' ({MovieId}) with {Rating} stars (previous: {PreviousRating})",
            domainEvent.UserId,
            movieTitle,
            domainEvent.MovieId,
            domainEvent.Rating,
            domainEvent.PreviousRating?.ToString() ?? "none"
        );
    }
    
    public async Task HandleAsync(MovieFavoritedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByTmdbIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        var action = domainEvent.IsFavorite ? "favorited" : "unfavorited";
        
        _logger.LogInformation(
            "User {UserId} {Action} movie '{MovieTitle}' ({MovieId})",
            domainEvent.UserId,
            action,
            movieTitle,
            domainEvent.MovieId
        );
    }
    
    public async Task HandleAsync(StatisticsInvalidatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Statistics cache invalidated for user {UserId}", domainEvent.UserId);
        await Task.CompletedTask;
    }
}

