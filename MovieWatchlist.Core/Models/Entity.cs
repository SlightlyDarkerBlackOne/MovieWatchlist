using MovieWatchlist.Core.Events;

namespace MovieWatchlist.Core.Models;

public abstract class Entity
{
    private readonly List<IDomainEvent> m_domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => m_domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        m_domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        m_domainEvents.Clear();
    }
}

