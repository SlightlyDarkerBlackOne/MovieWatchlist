namespace MovieWatchlist.Core.Models;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private RefreshToken() { }

    /// <summary>
    /// Factory method for creating new refresh tokens
    /// </summary>
    public static RefreshToken Create(int userId, string token, int expirationDays)
    {
        if (userId < 0)
            throw new ArgumentException("User ID cannot be negative", nameof(userId));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        if (expirationDays <= 0)
            throw new ArgumentException("Expiration days must be greater than zero", nameof(expirationDays));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    /// <summary>
    /// Revokes this refresh token
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
    }

    /// <summary>
    /// Checks if the token is valid (not revoked and not expired)
    /// </summary>
    public bool IsValid() => !IsRevoked && !IsExpired();

    /// <summary>
    /// Checks if the token has expired
    /// </summary>
    public bool IsExpired() => ExpiresAt <= DateTime.UtcNow;
}
