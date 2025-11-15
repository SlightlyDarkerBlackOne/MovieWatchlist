using FluentAssertions;
using Mapster;
using Moq;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Movies.Queries;

public class GetPopularMoviesQueryHandlerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly GetPopularMoviesQueryHandler _handler;

    static GetPopularMoviesQueryHandlerTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);
    }

    public GetPopularMoviesQueryHandlerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _handler = new GetPopularMoviesQueryHandler(_mockTmdbService.Object);
    }

    [Fact]
    public async Task Handle_WithValidPage_ReturnsMovies()
    {
        var query = new GetPopularMoviesQuery(Page: 1);
        var movies = new List<Movie>
        {
            Movie().WithTitle("Popular Movie 1").Build(),
            Movie().WithTitle("Popular Movie 2").Build()
        };

        _mockTmdbService
            .Setup(x => x.GetPopularMoviesAsync(1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithPageLessThanOne_UsesPageOne()
    {
        var query = new GetPopularMoviesQuery(Page: 0);
        var movies = new List<Movie>
        {
            Movie().Build()
        };

        _mockTmdbService
            .Setup(x => x.GetPopularMoviesAsync(1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockTmdbService.Verify(x => x.GetPopularMoviesAsync(1), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNegativePage_UsesPageOne()
    {
        var query = new GetPopularMoviesQuery(Page: -5);
        var movies = new List<Movie>
        {
            Movie().Build()
        };

        _mockTmdbService
            .Setup(x => x.GetPopularMoviesAsync(1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockTmdbService.Verify(x => x.GetPopularMoviesAsync(1), Times.Once);
    }
}

