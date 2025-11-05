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
    
    public UpdateStatisticsHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
        }
    }
}

