using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Events;

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

        await Task.CompletedTask;
    }
}

