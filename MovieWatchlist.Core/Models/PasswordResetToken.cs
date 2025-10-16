namespace MovieWatchlist.Core.Models;

/// <summary>
/// Represents a password reset token
/// </summary>
public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsUsed { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}

