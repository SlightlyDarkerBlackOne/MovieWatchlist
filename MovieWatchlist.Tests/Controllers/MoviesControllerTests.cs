using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieWatchlist.Api.Controllers;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using Xunit;

namespace MovieWatchlist.Tests.Controllers;

public class MoviesControllerTests
{
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly Mock<IRepository<Movie>> _mockMovieRepository;
    private readonly MoviesController _controller;

    public MoviesControllerTests()
    {
        _mockTmdbService = new Mock<ITmdbService>();
        _mockMovieRepository = new Mock<IRepository<Movie>>();
        _controller = new MoviesController(_mockTmdbService.Object, _mockMovieRepository.Object);
    }

    [Fact]
    public async Task SearchMovies_ValidQuery_ReturnsOkResult()
    {
        // Arrange
        var expectedMovies = new List<Movie>
        {
            new()
            {
                TmdbId = 1,
                Title = "Test Movie",
                Overview = "Test Overview"
            }
        };

        _mockTmdbService
            .Setup(x => x.SearchMoviesAsync("test", 1))
            .ReturnsAsync(expectedMovies);

        // Act
        var result = await _controller.SearchMovies("test");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task SearchMovies_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchMovies("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMovieDetails_ValidId_ReturnsOkResult()
    {
        // Arrange
        var expectedMovie = new Movie
        {
            TmdbId = 1,
            Title = "Test Movie",
            Overview = "Test Overview"
        };

        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync(expectedMovie);

        // Act
        var result = await _controller.GetMovieDetails(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<Movie>(okResult.Value);
        Assert.Equal(1, returnValue.TmdbId);
    }

    [Fact]
    public async Task GetMovieDetails_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await _controller.GetMovieDetails(1);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
} 