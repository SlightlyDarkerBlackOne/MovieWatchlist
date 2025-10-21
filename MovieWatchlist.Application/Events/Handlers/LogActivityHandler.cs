using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class LogActivityHandler :
    IDomainEventHandler<MovieWatchedEvent>,
    IDomainEventHandler<MovieRatedEvent>,
    IDomainEventHandler<MovieFavoritedEvent>
{
    private readonly ILogger<LogActivityHandler> _logger;
    private readonly IMovieRepository _movieRepository;
    
    public LogActivityHandler(ILogger<LogActivityHandler> logger, IMovieRepository movieRepository)
    {
        _logger = logger;
        _movieRepository = movieRepository;
    }
    
    public async Task HandleAsync(MovieWatchedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(domainEvent.MovieId);
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
        var movie = await _movieRepository.GetByIdAsync(domainEvent.MovieId);
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
        var movie = await _movieRepository.GetByIdAsync(domainEvent.MovieId);
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
}

