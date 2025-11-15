using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyRecommendedMovies;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyRecommendedMoviesQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyRecommendedMoviesQueryHandler _handler;

    public GetMyRecommendedMoviesQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyRecommendedMoviesQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsRecommendedMovies()
    {
        var query = new GetMyRecommendedMoviesQuery();
        var recommendedMovies = new List<Movie>
        {
            Movie().WithTitle("Recommended Movie 1").Build(),
            Movie().WithTitle("Recommended Movie 2").Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetRecommendedMoviesAsync(query))
            .ReturnsAsync(recommendedMovies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoRecommendations_ReturnsEmptyList()
    {
        var query = new GetMyRecommendedMoviesQuery();

        _mockWatchlistService
            .Setup(x => x.GetRecommendedMoviesAsync(query))
            .ReturnsAsync(new List<Movie>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

