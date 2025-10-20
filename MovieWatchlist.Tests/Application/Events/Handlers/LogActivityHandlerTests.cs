using Microsoft.Extensions.Logging;
using MovieWatchlist.Application.Events.Handlers;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using Moq;
using Xunit;

namespace MovieWatchlist.Tests.Application.Events.Handlers;

public class LogActivityHandlerTests
{
    private readonly Mock<ILogger<LogActivityHandler>> m_mockLogger;
    private readonly Mock<IMovieRepository> m_mockMovieRepository;
    private readonly LogActivityHandler m_handler;
    
    public LogActivityHandlerTests()
    {
        m_mockLogger = new Mock<ILogger<LogActivityHandler>>();
        m_mockMovieRepository = new Mock<IMovieRepository>();
        m_handler = new LogActivityHandler(m_mockLogger.Object, m_mockMovieRepository.Object);
    }
    
    [Fact]
    public async Task HandleAsync_MovieWatchedEvent_LogsCorrectInformation()
    {
        // Arrange
        var movie = new Movie
        {
            Id = 123,
            Title = "Inception",
            TmdbId = 27205
        };
        
        m_mockMovieRepository
            .Setup(x => x.GetByIdAsync(123))
            .ReturnsAsync(movie);
        
        var domainEvent = new MovieWatchedEvent(1, 123, DateTime.UtcNow);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockMovieRepository.Verify(x => x.GetByIdAsync(123), Times.Once);
        
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User 1") && 
                                               v.ToString()!.Contains("Inception") && 
                                               v.ToString()!.Contains("123")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieWatchedEvent_WithMissingMovie_LogsUnknownMovie()
    {
        // Arrange
        m_mockMovieRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Movie?)null);
        
        var domainEvent = new MovieWatchedEvent(1, 999, DateTime.UtcNow);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown Movie")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieRatedEvent_LogsCorrectInformation()
    {
        // Arrange
        var movie = new Movie
        {
            Id = 456,
            Title = "The Dark Knight",
            TmdbId = 155
        };
        
        m_mockMovieRepository
            .Setup(x => x.GetByIdAsync(456))
            .ReturnsAsync(movie);
        
        var domainEvent = new MovieRatedEvent(1, 456, 9, 8);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User 1") && 
                                               v.ToString()!.Contains("The Dark Knight") && 
                                               v.ToString()!.Contains("456") &&
                                               v.ToString()!.Contains("9") &&
                                               v.ToString()!.Contains("8")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieRatedEvent_WithNullPreviousRating_LogsNone()
    {
        // Arrange
        var movie = new Movie { Id = 789, Title = "Test Movie" };
        m_mockMovieRepository.Setup(x => x.GetByIdAsync(789)).ReturnsAsync(movie);
        
        var domainEvent = new MovieRatedEvent(1, 789, 7, null);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("none")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieFavoritedEvent_True_LogsFavorited()
    {
        // Arrange
        var movie = new Movie { Id = 111, Title = "Pulp Fiction" };
        m_mockMovieRepository.Setup(x => x.GetByIdAsync(111)).ReturnsAsync(movie);
        
        var domainEvent = new MovieFavoritedEvent(1, 111, true);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("favorited")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieFavoritedEvent_False_LogsUnfavorited()
    {
        // Arrange
        var movie = new Movie { Id = 222, Title = "Test Movie" };
        m_mockMovieRepository.Setup(x => x.GetByIdAsync(222)).ReturnsAsync(movie);
        
        var domainEvent = new MovieFavoritedEvent(1, 222, false);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unfavorited")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MultipleEvents_LogsEachEvent()
    {
        // Arrange
        var movie = new Movie { Id = 333, Title = "Test Movie" };
        m_mockMovieRepository.Setup(x => x.GetByIdAsync(333)).ReturnsAsync(movie);
        
        var watchedEvent = new MovieWatchedEvent(1, 333, DateTime.UtcNow);
        var ratedEvent = new MovieRatedEvent(1, 333, 8, 7);
        var favoritedEvent = new MovieFavoritedEvent(1, 333, true);
        
        // Act
        await m_handler.HandleAsync(watchedEvent);
        await m_handler.HandleAsync(ratedEvent);
        await m_handler.HandleAsync(favoritedEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }
}

