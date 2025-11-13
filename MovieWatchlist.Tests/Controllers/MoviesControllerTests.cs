using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieWatchlist.Api.Controllers;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;
using MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using Xunit;
using static MovieWatchlist.Tests.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Tests.Controllers;

public class MoviesControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly MoviesController _controller;

    public MoviesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new MoviesController(_mockMediator.Object);
    }

    [Fact]
    public async Task SearchMovies_ValidQuery_ReturnsOkResult()
    {
        var expectedMovies = new List<MovieDetailsDto>
        {
            MovieDetailsDto().Build()
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<SearchMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MovieDetailsDto>>.Success(expectedMovies));

        var result = await _controller.SearchMovies("test");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task SearchMovies_EmptyQuery_ReturnsOkWithFailureResult()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<SearchMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MovieDetailsDto>>.Failure(ErrorMessages.SearchQueryRequired));

        var result = await _controller.SearchMovies("");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task GetMovieDetails_ValidId_ReturnsOkResult()
    {
        var expectedMovie = MovieDetailsDto().Build();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieDetailsDto>.Success(expectedMovie));

        var result = await _controller.GetMovieDetails(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MovieDetailsDto>(okResult.Value);
        Assert.Equal(1, returnValue.TmdbId);
    }

    [Fact]
    public async Task GetMovieDetails_InvalidId_ReturnsOkWithFailureResult()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieDetailsDto>.Failure("Movie not found"));

        var result = await _controller.GetMovieDetails(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task GetPopularMovies_ReturnsOkResult()
    {
        var expectedMovies = new List<MovieDetailsDto>
        {
            MovieDetailsDto()
                .WithTitle("Popular Movie")
                .WithBackdropPath(null)
                .Build()
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetPopularMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MovieDetailsDto>>.Success(expectedMovies));

        var result = await _controller.GetPopularMovies();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetMoviesByGenre_ValidGenre_ReturnsOkResult()
    {
        var expectedMovies = new List<MovieDetailsDto>
        {
            MovieDetailsDto()
                .WithTitle("Action Movie")
                .WithBackdropPath(null)
                .WithGenres("Action")
                .Build()
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMoviesByGenreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MovieDetailsDto>>.Success(expectedMovies));

        var result = await _controller.GetMoviesByGenre("Action");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetMoviesByGenre_InvalidGenre_ReturnsOkWithFailureResult()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMoviesByGenreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<MovieDetailsDto>>.Failure("Invalid genre"));

        var result = await _controller.GetMoviesByGenre("InvalidGenre");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_ValidId_ReturnsOkResult()
    {
        var expectedMovie = MovieDetailsDto()
            .WithBackdropPath(null)
            .Build();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieDetailsDto>.Success(expectedMovie));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MovieDetailsDto>(okResult.Value);
        Assert.Equal(1, returnValue.TmdbId);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_InvalidId_ReturnsOkWithFailureResult()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieDetailsDto>.Failure("Movie with TMDB ID 1 not found"));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_RateLimit_ReturnsOkWithFailureResult()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieDetailsDto>.Failure(ErrorMessages.TmdbRateLimitExceeded));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }
} 