using FluentAssertions;
using Mapster;
using Moq;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Application.UnitTests.Features.Movies.Queries;

public class SearchMoviesQueryHandlerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly SearchMoviesQueryHandler _handler;

    static SearchMoviesQueryHandlerTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);
    }

    public SearchMoviesQueryHandlerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _handler = new SearchMoviesQueryHandler(_mockTmdbService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsMovies()
    {
        var query = new SearchMoviesQuery(Query: TestConstants.Tmdb.TestMovieTitle, Page: 1);
        var movies = new List<Core.Models.Movie>
        {
            Movie()
                .WithTitle(TestConstants.Tmdb.TestMovieTitle)
                .WithTmdbId(27205)
                .Build()
        };

        _mockTmdbService
            .Setup(x => x.SearchMoviesAsync(TestConstants.Tmdb.TestMovieTitle, 1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().Title.Should().Be(TestConstants.Tmdb.TestMovieTitle);
    }

    [Fact]
    public async Task Handle_WithMultipleMovies_ReturnsMoviesSortedByVoteCountDescending()
    {
        var query = new SearchMoviesQuery(Query: TestConstants.Tmdb.TestMovieTitle, Page: 1);
        var movies = new List<Core.Models.Movie>
        {
            Movie()
                .WithTitle("Movie A")
                .WithTmdbId(1)
                .WithVoteCount(100)
                .Build(),
            Movie()
                .WithTitle("Movie B")
                .WithTmdbId(2)
                .WithVoteCount(500)
                .Build(),
            Movie()
                .WithTitle("Movie C")
                .WithTmdbId(3)
                .WithVoteCount(200)
                .Build()
        };

        _mockTmdbService
            .Setup(x => x.SearchMoviesAsync(TestConstants.Tmdb.TestMovieTitle, 1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value!.ElementAt(0).Title.Should().Be("Movie B");
        result.Value!.ElementAt(1).Title.Should().Be("Movie C");
        result.Value!.ElementAt(2).Title.Should().Be("Movie A");
    }

    [Fact]
    public async Task Handle_WithEmptyQuery_ReturnsFailure()
    {
        var query = new SearchMoviesQuery(Query: "", Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.SearchQueryRequired);
        
        _mockTmdbService.Verify(x => x.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWhitespaceQuery_ReturnsFailure()
    {
        var query = new SearchMoviesQuery(Query: "   ", Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.SearchQueryRequired);
    }

    [Fact]
    public async Task Handle_WithNullQuery_ReturnsFailure()
    {
        var query = new SearchMoviesQuery(Query: null!, Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.SearchQueryRequired);
    }
}

