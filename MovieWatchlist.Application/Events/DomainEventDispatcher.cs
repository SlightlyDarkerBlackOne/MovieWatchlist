using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider m_serviceProvider;
    
    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        m_serviceProvider = serviceProvider;
    }
    
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = m_serviceProvider.GetServices(handlerType);
        
        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                var task = (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                await task;
            }
        }
    }
    
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}

