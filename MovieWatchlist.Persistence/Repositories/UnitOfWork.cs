using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Persistence.Data;

namespace MovieWatchlist.Persistence.Repositories;

/// <summary>
/// Unit of Work pattern implementation for managing transactions across multiple repositories.
/// Ensures that all changes are committed together or rolled back if an error occurs.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MovieWatchlistDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private bool _disposed = false;

    public UnitOfWork(MovieWatchlistDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the database asynchronously.
    /// Dispatches domain events after successful save.
    /// </summary>
    /// <returns>The number of state entries written to the database</returns>
    public async Task<int> SaveChangesAsync()
    {
        var entitiesWithEvents = _context.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();
        
        var result = await _context.SaveChangesAsync();
        
        await _domainEventDispatcher.DispatchAsync(domainEvents);
        
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
        
        return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

