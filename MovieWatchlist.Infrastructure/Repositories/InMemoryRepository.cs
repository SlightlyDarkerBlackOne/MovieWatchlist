using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public Task<T?> GetByIdAsync(int id)
    {
        // Use reflection to find the Id property
        var item = _items.FirstOrDefault(GetIdPredicate(id));
        return Task.FromResult(item);
    }

    private Func<T, bool> GetIdPredicate(int id)
    {
        return item =>
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(item);
                return value != null && value.Equals(id);
            }
            return false;
        };
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_items.AsEnumerable());
    }

    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var func = predicate.Compile();
        return Task.FromResult(_items.Where(func));
    }

    public Task AddAsync(T entity)
    {
        // Set the ID if the entity has an Id property
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            var currentId = idProperty.GetValue(entity);
            if (currentId == null || currentId.Equals(0))
            {
                idProperty.SetValue(entity, _nextId++);
            }
        }
        
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        // Find the existing item and replace it
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            var entityId = idProperty.GetValue(entity);
            var index = _items.FindIndex(item => 
            {
                var itemId = idProperty.GetValue(item);
                return itemId != null && itemId.Equals(entityId);
            });
            
            if (index >= 0)
            {
                _items[index] = entity;
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(int id)
    {
        var exists = _items.Any(GetIdPredicate(id));
        return Task.FromResult(exists);
    }
} 