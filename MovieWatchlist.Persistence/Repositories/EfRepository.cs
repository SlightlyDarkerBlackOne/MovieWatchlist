using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Persistence.Data;

namespace MovieWatchlist.Persistence.Repositories;

/// <summary>
/// Entity Framework-based implementation of the generic repository pattern.
/// Provides data access operations using Entity Framework Core with PostgreSQL.
/// </summary>
/// <typeparam name="T">The entity type this repository manages</typeparam>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly MovieWatchlistDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(MovieWatchlistDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, null otherwise</returns>
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all entities of type T asynchronously.
    /// </summary>
    /// <returns>A collection of all entities</returns>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Adds a new entity to the repository asynchronously.
    /// Note: Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Updates an existing entity in the repository.
    /// Note: Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity from the repository.
    /// Note: Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if an entity with the specified ID exists asynchronously.
    /// </summary>
    /// <param name="id">The ID to check for existence</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
