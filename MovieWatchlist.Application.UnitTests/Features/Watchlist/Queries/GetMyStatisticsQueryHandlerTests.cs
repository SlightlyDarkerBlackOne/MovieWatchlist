using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyStatistics;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyStatisticsQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyStatisticsQueryHandler _handler;

    public GetMyStatisticsQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyStatisticsQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsStatistics()
    {
        var query = new GetMyStatisticsQuery();
        var statistics = new WatchlistStatistics(
            TotalMovies: 10,
            WatchedMovies: 5,
            PlannedMovies: 3,
            FavoriteMovies: 4,
            AverageUserRating: 7.5,
            AverageTmdbRating: 8.0,
            MostWatchedGenre: "Action",
            MoviesThisYear: 2,
            GenreBreakdown: new Dictionary<string, int> { { "Action", 5 } },
            YearlyBreakdown: new Dictionary<int, int> { { 2023, 2 } }
        );

        _mockWatchlistService
            .Setup(x => x.GetUserStatisticsAsync(query))
            .ReturnsAsync(statistics);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalMovies.Should().Be(10);
        result.WatchedMovies.Should().Be(5);
        result.AverageUserRating.Should().Be(7.5);
    }
}

