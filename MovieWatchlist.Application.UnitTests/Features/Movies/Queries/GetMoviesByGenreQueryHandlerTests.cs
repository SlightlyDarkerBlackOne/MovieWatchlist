using FluentAssertions;
using Mapster;
using Moq;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Movies.Queries;

public class GetMoviesByGenreQueryHandlerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly GetMoviesByGenreQueryHandler _handler;

    static GetMoviesByGenreQueryHandlerTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);
    }

    public GetMoviesByGenreQueryHandlerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _handler = new GetMoviesByGenreQueryHandler(_mockTmdbService.Object);
    }

    [Fact]
    public async Task Handle_WithValidGenre_ReturnsMovies()
    {
        var query = new GetMoviesByGenreQuery(Genre: GenreConstants.Action, Page: 1);
        var movies = new List<Movie>
        {
            Movie().WithTitle("Action Movie 1").WithGenres(GenreConstants.Action).Build(),
            Movie().WithTitle("Action Movie 2").WithGenres(GenreConstants.Action).Build()
        };

        _mockTmdbService
            .Setup(x => x.GetMoviesByGenreAsync(GenreConstants.Action, 1))
            .ReturnsAsync(movies);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithEmptyGenre_ReturnsFailure()
    {
        var query = new GetMoviesByGenreQuery(Genre: "", Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.GenreRequired);
        
        _mockTmdbService.Verify(x => x.GetMoviesByGenreAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWhitespaceGenre_ReturnsFailure()
    {
        var query = new GetMoviesByGenreQuery(Genre: "   ", Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.GenreRequired);
    }

    [Fact]
    public async Task Handle_WithNullGenre_ReturnsFailure()
    {
        var query = new GetMoviesByGenreQuery(Genre: null!, Page: 1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.GenreRequired);
    }

    [Fact]
    public async Task Handle_WithInvalidGenre_HandlesArgumentException()
    {
        var query = new GetMoviesByGenreQuery(Genre: TestConstants.Tmdb.InvalidGenre, Page: 1);

        _mockTmdbService
            .Setup(x => x.GetMoviesByGenreAsync(TestConstants.Tmdb.InvalidGenre, 1))
            .ThrowsAsync(new ArgumentException(TestConstants.Tmdb.InvalidGenreErrorMessage));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.Tmdb.InvalidGenreErrorMessage);
    }
}

