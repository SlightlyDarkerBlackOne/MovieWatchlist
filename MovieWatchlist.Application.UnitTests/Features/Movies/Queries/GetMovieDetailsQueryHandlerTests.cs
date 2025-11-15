using FluentAssertions;
using Mapster;
using Moq;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Movies.Queries;

public class GetMovieDetailsQueryHandlerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly GetMovieDetailsQueryHandler _handler;

    static GetMovieDetailsQueryHandlerTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);
    }

    public GetMovieDetailsQueryHandlerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _handler = new GetMovieDetailsQueryHandler(_mockTmdbService.Object);
    }

    [Fact]
    public async Task Handle_WithValidTmdbId_ReturnsMovieDetails()
    {
        var query = new GetMovieDetailsQuery(TmdbId: TestConstants.Movies.DefaultTmdbId);
        var movie = Movie()
            .WithTmdbId(TestConstants.Movies.DefaultTmdbId)
            .WithTitle(TestConstants.Movies.DefaultTitle)
            .Build();

        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(TestConstants.Movies.DefaultTmdbId))
            .ReturnsAsync(movie);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(TestConstants.Movies.DefaultTitle);
    }

    [Fact]
    public async Task Handle_WithNonExistentTmdbId_ReturnsFailure()
    {
        var query = new GetMovieDetailsQuery(TmdbId: 99999);

        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(99999))
            .ReturnsAsync((Movie?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.MovieNotFound);
    }
}

