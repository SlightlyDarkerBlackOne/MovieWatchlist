using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Infrastructure.Events;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;
using Moq;
using Xunit;

namespace MovieWatchlist.Tests.Application.Events;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_CallsAllRegisteredHandlers()
    {
        // Arrange
        var mockHandler1 = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        var mockHandler2 = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockHandler1.Object);
        serviceCollection.AddSingleton(mockHandler2.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var domainEvent = new MovieWatchedEvent(1, 123, DateTime.UtcNow);
        
        // Act
        await dispatcher.DispatchAsync(domainEvent);
        
        // Assert
        mockHandler1.Verify(x => x.HandleAsync(domainEvent, default), Times.Once);
        mockHandler2.Verify(x => x.HandleAsync(domainEvent, default), Times.Once);
    }
    
    [Fact]
    public async Task DispatchAsync_WithNoHandlers_DoesNotThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var domainEvent = new MovieWatchedEvent(1, 123, DateTime.UtcNow);
        
        // Act & Assert
        await dispatcher.DispatchAsync(domainEvent);
    }
    
    [Fact]
    public async Task DispatchAsync_WithMultipleEvents_CallsHandlersForEach()
    {
        // Arrange
        var mockHandler = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockHandler.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var events = new List<IDomainEvent>
        {
            new MovieWatchedEvent(1, 123, DateTime.UtcNow),
            new MovieWatchedEvent(1, 456, DateTime.UtcNow),
            new MovieWatchedEvent(2, 789, DateTime.UtcNow)
        };
        
        // Act
        await dispatcher.DispatchAsync(events);
        
        // Assert
        mockHandler.Verify(x => x.HandleAsync(It.IsAny<MovieWatchedEvent>(), default), Times.Exactly(3));
    }
    
    [Fact]
    public async Task DispatchAsync_WithDifferentEventTypes_CallsCorrectHandlers()
    {
        // Arrange
        var mockWatchedHandler = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        var mockRatedHandler = new Mock<IDomainEventHandler<MovieRatedEvent>>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockWatchedHandler.Object);
        serviceCollection.AddSingleton(mockRatedHandler.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var events = new List<IDomainEvent>
        {
            new MovieWatchedEvent(1, 123, DateTime.UtcNow),
            new MovieRatedEvent(1, 123, 8, 7),
            new MovieWatchedEvent(2, 456, DateTime.UtcNow)
        };
        
        // Act
        await dispatcher.DispatchAsync(events);
        
        // Assert
        mockWatchedHandler.Verify(x => x.HandleAsync(It.IsAny<MovieWatchedEvent>(), default), Times.Exactly(2));
        mockRatedHandler.Verify(x => x.HandleAsync(It.IsAny<MovieRatedEvent>(), default), Times.Once);
    }
    
    [Fact]
    public async Task DispatchAsync_WithCancellationToken_PassesToHandlers()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        var mockHandler = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockHandler.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var domainEvent = new MovieWatchedEvent(1, 123, DateTime.UtcNow);
        
        // Act
        await dispatcher.DispatchAsync(domainEvent, cancellationToken);
        
        // Assert
        mockHandler.Verify(x => x.HandleAsync(domainEvent, cancellationToken), Times.Once);
    }
    
    [Fact]
    public async Task DispatchAsync_WithHandlerException_PropagatesException()
    {
        // Arrange
        var mockHandler = new Mock<IDomainEventHandler<MovieWatchedEvent>>();
        mockHandler
            .Setup(x => x.HandleAsync(It.IsAny<MovieWatchedEvent>(), default))
            .ThrowsAsync(new InvalidOperationException("Handler error"));
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(mockHandler.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var dispatcher = new DomainEventDispatcher(serviceProvider);
        var domainEvent = new MovieWatchedEvent(1, 123, DateTime.UtcNow);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.DispatchAsync(domainEvent));
    }
}

