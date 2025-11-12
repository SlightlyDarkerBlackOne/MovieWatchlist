using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieWatchlist.Api.Controllers;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;
using Xunit;

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
        var expectedMovies = new List<Movie>
        {
            new()
            {
                TmdbId = 1,
                Title = "Test Movie",
                Overview = "Test Overview"
            }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<SearchMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Movie>>.Success(expectedMovies));

        var result = await _controller.SearchMovies("test");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task SearchMovies_EmptyQuery_ReturnsBadRequest()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<SearchMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Movie>>.Failure(ErrorMessages.SearchQueryRequired));

        var result = await _controller.SearchMovies("");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMovieDetails_ValidId_ReturnsOkResult()
    {
        var expectedMovie = new Movie
        {
            TmdbId = 1,
            Title = "Test Movie",
            Overview = "Test Overview"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Movie>.Success(expectedMovie));

        var result = await _controller.GetMovieDetails(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MovieDetailsDto>(okResult.Value);
        Assert.Equal(1, returnValue.TmdbId);
    }

    [Fact]
    public async Task GetMovieDetails_InvalidId_ReturnsNotFound()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Movie>.Failure("Movie not found"));

        var result = await _controller.GetMovieDetails(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetPopularMovies_ReturnsOkResult()
    {
        var expectedMovies = new List<Movie>
        {
            new() { TmdbId = 1, Title = "Popular Movie" }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetPopularMoviesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Movie>>.Success(expectedMovies));

        var result = await _controller.GetPopularMovies();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetMoviesByGenre_ValidGenre_ReturnsOkResult()
    {
        var expectedMovies = new List<Movie>
        {
            new() { TmdbId = 1, Title = "Action Movie", Genres = new[] { "Action" } }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMoviesByGenreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Movie>>.Success(expectedMovies));

        var result = await _controller.GetMoviesByGenre("Action");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MovieDetailsDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetMoviesByGenre_InvalidGenre_ReturnsBadRequest()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMoviesByGenreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Movie>>.Failure("Invalid genre"));

        var result = await _controller.GetMoviesByGenre("InvalidGenre");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_ValidId_ReturnsOkResult()
    {
        var expectedMovie = new Movie
        {
            TmdbId = 1,
            Title = "Test Movie",
            CreditsJson = "{\"cast\":[],\"crew\":[]}",
            VideosJson = "[]"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Movie>.Success(expectedMovie));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<MovieDetailsDto>(okResult.Value);
        Assert.Equal(1, returnValue.TmdbId);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_InvalidId_ReturnsNotFound()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Movie>.Failure("Movie with TMDB ID 1 not found"));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMovieDetailsByTmdbId_RateLimit_Returns429()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetMovieDetailsByTmdbIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Movie>.Failure(ErrorMessages.TmdbRateLimitExceeded));

        var result = await _controller.GetMovieDetailsByTmdbId(1);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(429, statusResult.StatusCode);
    }
} 