using Microsoft.Extensions.Logging;
using MovieWatchlist.Infrastructure.Events;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using Moq;
using Xunit;

namespace MovieWatchlist.Application.UnitTests.Events.Handlers;

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
            Title = "Inception",
            TmdbId = 27205
        };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 123);
        
        m_mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(27205))
            .ReturnsAsync(movie);
        
        var domainEvent = new MovieWatchedEvent(1, 27205, DateTime.UtcNow);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockMovieRepository.Verify(x => x.GetByTmdbIdAsync(27205), Times.Once);
        
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User 1") && 
                                               v.ToString()!.Contains("Inception") && 
                                               v.ToString()!.Contains("27205")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task HandleAsync_MovieWatchedEvent_WithMissingMovie_LogsUnknownMovie()
    {
        // Arrange
        m_mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(999))
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
            Title = "The Dark Knight",
            TmdbId = 155
        };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 456);
        
        m_mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(155))
            .ReturnsAsync(movie);
        
        var domainEvent = new MovieRatedEvent(1, 155, 9, 8);
        
        // Act
        await m_handler.HandleAsync(domainEvent);
        
        // Assert
        m_mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User 1") && 
                                               v.ToString()!.Contains("The Dark Knight") && 
                                               v.ToString()!.Contains("155") &&
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
        var movie = new Movie { Title = "Test Movie", TmdbId = 789 };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 789);
        m_mockMovieRepository.Setup(x => x.GetByTmdbIdAsync(789)).ReturnsAsync(movie);
        
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
        var movie = new Movie { Title = "Pulp Fiction", TmdbId = 111 };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 111);
        m_mockMovieRepository.Setup(x => x.GetByTmdbIdAsync(111)).ReturnsAsync(movie);
        
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
        var movie = new Movie { Title = "Test Movie", TmdbId = 222 };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 222);
        m_mockMovieRepository.Setup(x => x.GetByTmdbIdAsync(222)).ReturnsAsync(movie);
        
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
        var movie = new Movie { Title = "Test Movie", TmdbId = 333 };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 333);
        m_mockMovieRepository.Setup(x => x.GetByTmdbIdAsync(333)).ReturnsAsync(movie);
        
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

