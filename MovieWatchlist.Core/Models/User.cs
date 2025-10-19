using System;
using System.Collections.Generic;

namespace MovieWatchlist.Core.Models;

public class User
{
    // Public properties with private setters for encapsulation
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>(); // Navigation property - kept public for EF Core

    // Private parameterless constructor for EF Core
    private User() { }

    // Factory method for creating new users
    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
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

    // Domain methods for state changes

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
    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be null or empty", nameof(newEmail));

        Email = newEmail;
    }
} 