using System.Threading.Tasks;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Core.Interfaces;

/// <summary>
/// Repository interface for User entity with domain-specific operations.
/// Extends the generic repository with user-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Finds a user by their username asynchronously.
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByUsernameAsync(Username username);

    /// <summary>
    /// Finds a user by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(Email email);

    /// <summary>
    /// Checks if a username is already taken by another user.
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates)</param>
    /// <returns>True if username is taken, false otherwise</returns>
    Task<bool> IsUsernameTakenAsync(Username username, int? excludeUserId = null);

    /// <summary>
    /// Checks if an email address is already taken by another user.
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates)</param>
    /// <returns>True if email is taken, false otherwise</returns>
    Task<bool> IsEmailTakenAsync(Email email, int? excludeUserId = null);

    /// <summary>
    /// Updates the last login timestamp for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <returns>True if the user was found and updated, false otherwise</returns>
    Task<bool> UpdateLastLoginAsync(int userId);
}


