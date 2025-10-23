using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Events;

public record UserRegisteredEvent(
    int UserId,
    string Username,
    string Email
) : DomainEvent;

public record UserLoggedInEvent(
    int UserId,
    string Username,
    string Email,
    DateTime LoginTime
) : DomainEvent;

public record RefreshTokenCreatedEvent(
    int UserId,
    string Token,
    DateTime ExpiresAt
) : DomainEvent;

public record UserPasswordChangedEvent(
    int UserId,
    string Username,
    string Email,
    DateTime ChangedAt
) : DomainEvent;
