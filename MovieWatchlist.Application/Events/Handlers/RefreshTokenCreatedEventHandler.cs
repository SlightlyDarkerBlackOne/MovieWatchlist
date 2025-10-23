using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class RefreshTokenCreatedEventHandler : IDomainEventHandler<RefreshTokenCreatedEvent>
{
    private readonly ILogger<RefreshTokenCreatedEventHandler> _logger;

    public RefreshTokenCreatedEventHandler(ILogger<RefreshTokenCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(RefreshTokenCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refresh token created for user: {UserId}, ExpiresAt: {ExpiresAt}", 
            domainEvent.UserId, domainEvent.ExpiresAt);

        // TODO: Log token creation for security auditing
        // TODO: Track token usage patterns
        // TODO: Monitor for unusual token creation frequency
        
        await Task.CompletedTask;
    }
}
