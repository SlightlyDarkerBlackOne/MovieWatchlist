using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events.Handlers;

public class UserLoggedInEventHandler : IDomainEventHandler<UserLoggedInEvent>
{
    private readonly ILogger<UserLoggedInEventHandler> _logger;

    public UserLoggedInEventHandler(ILogger<UserLoggedInEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserLoggedInEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User logged in: {UserId}, Username: {Username}, Email: {Email}, LoginTime: {LoginTime}", 
            domainEvent.UserId, domainEvent.Username, domainEvent.Email, domainEvent.LoginTime);

        // TODO: Log login activity for analytics
        // TODO: Update user session tracking
        // TODO: Check for suspicious login patterns
        
        await Task.CompletedTask;
    }
}
