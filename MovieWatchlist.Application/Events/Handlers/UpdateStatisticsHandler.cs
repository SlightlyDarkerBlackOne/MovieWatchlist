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
    private readonly IUserRepository m_userRepository;
    private readonly ILogger<UpdateStatisticsHandler> m_logger;
    
    public UpdateStatisticsHandler(
        IUserRepository userRepository,
        ILogger<UpdateStatisticsHandler> logger)
    {
        m_userRepository = userRepository;
        m_logger = logger;
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
        var user = await m_userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.InvalidateStatistics();
            await m_userRepository.UpdateAsync(user);
            
            m_logger.LogDebug("Invalidated statistics cache for user {UserId}", userId);
        }
    }
}

