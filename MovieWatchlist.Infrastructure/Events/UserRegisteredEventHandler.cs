using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Events;

public class UserRegisteredEventHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User registered: {UserId}, Username: {Username}, Email: {Email}", 
            domainEvent.UserId, domainEvent.Username, domainEvent.Email);

        await Task.CompletedTask;
    }
}

