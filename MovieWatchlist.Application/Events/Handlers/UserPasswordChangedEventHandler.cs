using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class UserPasswordChangedEventHandler : IDomainEventHandler<UserPasswordChangedEvent>
{
    private readonly ILogger<UserPasswordChangedEventHandler> _logger;

    public UserPasswordChangedEventHandler(ILogger<UserPasswordChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserPasswordChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password changed for user: {UserId}, Username: {Username}, Email: {Email}, ChangedAt: {ChangedAt}", 
            domainEvent.UserId, domainEvent.Username, domainEvent.Email, domainEvent.ChangedAt);

        // TODO: Send password change notification email
        // TODO: Log security event for auditing
        // TODO: Revoke all existing refresh tokens for security
        
        await Task.CompletedTask;
    }
}
