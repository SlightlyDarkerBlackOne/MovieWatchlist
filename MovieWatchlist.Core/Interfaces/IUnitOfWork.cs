namespace MovieWatchlist.Core.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions and ensuring data consistency.
/// Provides methods to save changes and manage database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database asynchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync();
}
