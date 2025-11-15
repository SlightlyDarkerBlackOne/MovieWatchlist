using FluentAssertions;
using Mapster;
using Moq;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Movies.Queries;

public class GetMovieDetailsByTmdbIdQueryHandlerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly Mock<IMovieRepository> _mockMovieRepository;
    private readonly GetMovieDetailsByTmdbIdQueryHandler _handler;

    static GetMovieDetailsByTmdbIdQueryHandlerTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MovieMappingProfile).Assembly);
    }

    public GetMovieDetailsByTmdbIdQueryHandlerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _mockMovieRepository = new Mock<IMovieRepository>();
        
        _handler = new GetMovieDetailsByTmdbIdQueryHandler(
            _mockTmdbService.Object,
            _mockMovieRepository.Object);
    }

    [Fact]
    public async Task Handle_WithCachedMovieWithSupplementalData_ReturnsCachedMovie()
    {
        var query = new GetMovieDetailsByTmdbIdQuery(TmdbId: TestConstants.Movies.DefaultTmdbId);
        var cachedMovie = Movie()
            .WithTmdbId(TestConstants.Movies.DefaultTmdbId)
            .WithTitle(TestConstants.Movies.DefaultTitle)
            .Build();
        
        typeof(Movie).GetProperty("CreditsJson")!.SetValue(cachedMovie, "{\"cast\":[]}");
        typeof(Movie).GetProperty("VideosJson")!.SetValue(cachedMovie, "[]");

        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(TestConstants.Movies.DefaultTmdbId))
            .ReturnsAsync(cachedMovie);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(TestConstants.Movies.DefaultTitle);
        
        _mockTmdbService.Verify(x => x.GetMovieDetailsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoCachedMovie_FetchesFromTmdbAndSaves()
    {
        var query = new GetMovieDetailsByTmdbIdQuery(TmdbId: TestConstants.Movies.DefaultTmdbId);
        var movie = Movie()
            .WithTmdbId(TestConstants.Movies.DefaultTmdbId)
            .WithTitle(TestConstants.Movies.DefaultTitle)
            .Build();

        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(TestConstants.Movies.DefaultTmdbId))
            .ReturnsAsync((Movie?)null);

        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(TestConstants.Movies.DefaultTmdbId))
            .ReturnsAsync(movie);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        _mockMovieRepository.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTmdbId_ReturnsFailure()
    {
        var query = new GetMovieDetailsByTmdbIdQuery(TmdbId: 99999);

        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(99999))
            .ReturnsAsync((Movie?)null);

        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(99999))
            .ReturnsAsync((Movie?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99999");
    }
}

