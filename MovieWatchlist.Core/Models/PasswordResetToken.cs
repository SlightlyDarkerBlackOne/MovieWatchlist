namespace MovieWatchlist.Core.Models;

/// <summary>
/// Represents a password reset token
/// </summary>
public class PasswordResetToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }

    public User User { get; set; } = null!;

    private PasswordResetToken() { }

    /// <summary>
    /// Factory method for creating new password reset tokens
    /// </summary>
    public static PasswordResetToken Create(int userId, string token, int expirationHours)
    {
        if (userId < 0)
            throw new ArgumentException("User ID cannot be negative", nameof(userId));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        if (expirationHours <= 0)
            throw new ArgumentException("Expiration hours must be greater than zero", nameof(expirationHours));

        return new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };
    }

    /// <summary>
    /// Marks this token as used
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
    }

    /// <summary>
    /// Checks if the token is valid (not used and not expired)
    /// </summary>
    public bool IsValid() => !IsUsed && !IsExpired();

    /// <summary>
    /// Checks if the token has expired
    /// </summary>
    public bool IsExpired() => ExpiresAt <= DateTime.UtcNow;
}

