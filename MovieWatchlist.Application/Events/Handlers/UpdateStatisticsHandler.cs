using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class UpdateStatisticsHandler : 
    IDomainEventHandler<MovieWatchedEvent>,
    IDomainEventHandler<MovieRatedEvent>,
    IDomainEventHandler<MovieFavoritedEvent>,
    IDomainEventHandler<StatisticsInvalidatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateStatisticsHandler> _logger;
    
    public UpdateStatisticsHandler(
        IUserRepository userRepository,
        ILogger<UpdateStatisticsHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task HandleAsync(MovieWatchedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await InvalidateStatisticsAsync(domainEvent.UserId);
    }
    
    public async Task HandleAsync(MovieRatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await InvalidateStatisticsAsync(domainEvent.UserId);
    }
    
    public async Task HandleAsync(MovieFavoritedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await InvalidateStatisticsAsync(domainEvent.UserId);
    }
    
    public async Task HandleAsync(StatisticsInvalidatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await InvalidateStatisticsAsync(domainEvent.UserId);
    }
    
    private async Task InvalidateStatisticsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.InvalidateStatistics();
            await _userRepository.UpdateAsync(user);
            
            _logger.LogDebug("Invalidated statistics cache for user {UserId}", userId);
        }
    }
}

