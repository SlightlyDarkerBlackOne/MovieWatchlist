using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Persistence.Data;

namespace MovieWatchlist.Persistence.Repositories;

/// <summary>
/// Repository for User entity with domain-specific operations.
/// Extends the generic repository with user-specific queries and operations.
/// </summary>
public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(MovieWatchlistDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Finds a user by their username asynchronously.
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByUsernameAsync(Username username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Finds a user by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Checks if a username is already taken by another user.
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates)</param>
    /// <returns>True if username is taken, false otherwise</returns>
    public async Task<bool> IsUsernameTakenAsync(Username username, int? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.Username == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Checks if an email address is already taken by another user.
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates)</param>
    /// <returns>True if email is taken, false otherwise</returns>
    public async Task<bool> IsEmailTakenAsync(Email email, int? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Updates the last login timestamp for a user.
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <returns>True if the user was found and updated, false otherwise</returns>
    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user == null)
            return false;

        // Use domain method instead of direct property assignment
        user.UpdateLastLogin();
        _dbSet.Update(user);
        return true;
    }
}

