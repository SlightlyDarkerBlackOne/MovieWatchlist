using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Core.Models;

public class User : Entity
{
    public int Id { get; private set; }
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>(); // Navigation property - kept public for EF Core

    private User() { }

    public static User Create(Username username, Email email, string passwordHash)
    {
        if (username == null)
            throw new ArgumentNullException(nameof(username));
        if (email == null)
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the last login timestamp to the current UTC time.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the user's password hash.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the user's email address.
    /// Note: Validation should be performed in the service layer before calling this method.
    /// </summary>
    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new ArgumentNullException(nameof(newEmail));

        Email = newEmail;
    }
} 