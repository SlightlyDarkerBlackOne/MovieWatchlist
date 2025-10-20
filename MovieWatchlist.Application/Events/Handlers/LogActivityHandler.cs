using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class LogActivityHandler :
    IDomainEventHandler<MovieWatchedEvent>,
    IDomainEventHandler<MovieRatedEvent>,
    IDomainEventHandler<MovieFavoritedEvent>
{
    private readonly ILogger<LogActivityHandler> m_logger;
    private readonly IMovieRepository m_movieRepository;
    
    public LogActivityHandler(ILogger<LogActivityHandler> logger, IMovieRepository movieRepository)
    {
        m_logger = logger;
        m_movieRepository = movieRepository;
    }
    
    public async Task HandleAsync(MovieWatchedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await m_movieRepository.GetByIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        m_logger.LogInformation(
            "User {UserId} watched movie '{MovieTitle}' ({MovieId}) at {WatchedDate}",
            domainEvent.UserId,
            movieTitle,
            domainEvent.MovieId,
            domainEvent.WatchedDate
        );
    }
    
    public async Task HandleAsync(MovieRatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var movie = await m_movieRepository.GetByIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        
        m_logger.LogInformation(
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
        var movie = await m_movieRepository.GetByIdAsync(domainEvent.MovieId);
        var movieTitle = movie?.Title ?? "Unknown Movie";
        var action = domainEvent.IsFavorite ? "favorited" : "unfavorited";
        
        m_logger.LogInformation(
            "User {UserId} {Action} movie '{MovieTitle}' ({MovieId})",
            domainEvent.UserId,
            action,
            movieTitle,
            domainEvent.MovieId
        );
    }
}

