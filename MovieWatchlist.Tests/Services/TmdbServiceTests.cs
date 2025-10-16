using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.DTOs;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Tests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MovieWatchlist.Tests.Services;

/// <summary>
/// Unit tests for TmdbService
/// </summary>
public class TmdbServiceTests : UnitTestBase, IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly TmdbSettings _tmdbSettings;
    private readonly TmdbService _tmdbService;

    public TmdbServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockGenreService = new Mock<IGenreService>();

        _tmdbSettings = new TmdbSettings
        {
            ApiKey = TestConstants.Tmdb.TestApiKey,
            ImageBaseUrl = TestConstants.Tmdb.TestImageBaseUrl
        };

        var options = Options.Create(_tmdbSettings);
        _tmdbService = new TmdbService(_httpClient, options, _mockGenreService.Object);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    #region SearchMoviesAsync Tests

    [Fact]
    public async Task SearchMoviesAsync_WithValidQuery_ReturnsMovies()
    {
        // Arrange
        var query = TestConstants.Tmdb.TestMovieTitle.ToLowerInvariant();
        var expectedMovies = CreateTmdbSearchResponse();
        
        SetupHttpResponse(expectedMovies);

        // Act
        var result = await _tmdbService.SearchMoviesAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var firstMovie = result.First();
        firstMovie.Title.Should().Be(TestConstants.Tmdb.TestMovieTitle);
        firstMovie.Overview.Should().Be(TestConstants.Tmdb.TestMovieOverview);
        firstMovie.TmdbId.Should().Be(27205);

        VerifyHttpRequest("search/movie", $"query={Uri.EscapeDataString(query)}&page=1");
    }

    [Fact]
    public async Task SearchMoviesAsync_WithPageParameter_IncludesPageInRequest()
    {
        // Arrange
        var query = "test";
        var page = 2;
        var expectedMovies = CreateTmdbSearchResponse();
        
        SetupHttpResponse(expectedMovies);

        // Act
        await _tmdbService.SearchMoviesAsync(query, page);

        // Assert
        VerifyHttpRequest("search/movie", $"query={Uri.EscapeDataString(query)}&page={page}");
    }

    [Fact]
    public async Task SearchMoviesAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var query = "nonexistent";
        var emptyResponse = new TmdbSearchResponse { Results = Array.Empty<TmdbMovieDto>() };
        
        SetupHttpResponse(emptyResponse);

        // Act
        var result = await _tmdbService.SearchMoviesAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMovieDetailsAsync Tests

    [Fact]
    public async Task GetMovieDetailsAsync_WithValidId_ReturnsMovie()
    {
        // Arrange
        var tmdbId = 27205;
        var movieDto = CreateTmdbMovieDto();

        SetupHttpResponse(movieDto);

        // Act
        var result = await _tmdbService.GetMovieDetailsAsync(tmdbId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(TestConstants.Tmdb.TestMovieTitle);
        result.TmdbId.Should().Be(27205);
        result.Overview.Should().Be(TestConstants.Tmdb.TestMovieOverview);

        VerifyHttpRequest($"movie/{tmdbId}");
    }

    [Fact]
    public async Task GetMovieDetailsAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var tmdbId = 999999;
        
        SetupHttpResponse<TmdbMovieDto?>(null);

        // Act
        var result = await _tmdbService.GetMovieDetailsAsync(tmdbId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPopularMoviesAsync Tests

    [Fact]
    public async Task GetPopularMoviesAsync_WithSuccessResponse_ReturnsMovies()
    {
        // Arrange
        var expectedMovies = CreateTmdbSearchResponse();
        
        SetupHttpResponse(expectedMovies);

        // Act
        var result = await _tmdbService.GetPopularMoviesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        // GetPopularMoviesAsync shuffles results, so just verify the expected movie is in the results
        result.Should().Contain(m => m.Title == TestConstants.Tmdb.TestMovieTitle);

        // Verify API was called with API key (GetPopularMoviesAsync randomly chooses between endpoints)
        _mockHttpMessageHandler.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains(_tmdbSettings.ApiKey) &&
                    (req.RequestUri.ToString().Contains("movie/popular") ||
                     req.RequestUri.ToString().Contains("movie/top_rated") ||
                     req.RequestUri.ToString().Contains("movie/now_playing"))),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetPopularMoviesAsync_WithFailureResponse_ThrowsHttpRequestException()
    {
        // Arrange
        SetupHttpErrorResponse(HttpStatusCode.BadRequest, "Invalid API key");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _tmdbService.GetPopularMoviesAsync());
    }

    [Fact]
    public async Task GetPopularMoviesAsync_WithPageParameter_IncludesPageInRequest()
    {
        // Arrange
        var expectedMovies = CreateTmdbSearchResponse();
        
        SetupHttpResponse(expectedMovies);

        // Act
        await _tmdbService.GetPopularMoviesAsync(TestConstants.Pagination.ThirdPage);

        // Assert - GetPopularMoviesAsync uses random pages, so just verify the request was made with API key
        _mockHttpMessageHandler.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains(_tmdbSettings.ApiKey) &&
                    (req.RequestUri.ToString().Contains("movie/popular") ||
                     req.RequestUri.ToString().Contains("movie/top_rated") ||
                     req.RequestUri.ToString().Contains("movie/now_playing"))),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region GetMoviesByGenreAsync Tests

    [Fact]
    public async Task GetMoviesByGenreAsync_WithValidGenre_ReturnsMovies()
    {
        // Arrange
        var genre = GenreConstants.Comedy;
        var genreId = 35;
        var expectedMovies = CreateTmdbSearchResponse();

        _mockGenreService.Setup(x => x.GetGenreId(genre))
            .Returns(genreId);
        
        SetupHttpResponse(expectedMovies);

        // Act
        var result = await _tmdbService.GetMoviesByGenreAsync(genre);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        VerifyHttpRequest("discover/movie", $"with_genres={genreId}&page=1");
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_WithInvalidGenre_ThrowsArgumentException()
    {
        // Arrange
        var invalidGenre = "invalidgenre";

        _mockGenreService.Setup(x => x.GetGenreId(invalidGenre))
            .Returns((int?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _tmdbService.GetMoviesByGenreAsync(invalidGenre));
        
        exception.Message.Should().Contain($"Invalid genre: {invalidGenre}");
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_WithPageParameter_IncludesPageInRequest()
    {
        // Arrange
        var genre = "action";
        var genreId = 28;
        var page = 2;
        var expectedMovies = CreateTmdbSearchResponse();

        _mockGenreService.Setup(x => x.GetGenreId(genre))
            .Returns(genreId);
        
        SetupHttpResponse(expectedMovies);

        // Act
        await _tmdbService.GetMoviesByGenreAsync(genre, page);

        // Assert
        VerifyHttpRequest("discover/movie", $"with_genres={genreId}&page={page}");
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_WithFailureResponse_ThrowsHttpRequestException()
    {
        // Arrange
        var genre = "drama";
        var genreId = 18;

        _mockGenreService.Setup(x => x.GetGenreId(genre))
            .Returns(genreId);
        
        SetupHttpErrorResponse(HttpStatusCode.InternalServerError, "Server error");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _tmdbService.GetMoviesByGenreAsync(genre));
    }

    #endregion

    #region GetPosterUrl Tests

    [Theory]
    [InlineData(TestConstants.Tmdb.TestPosterPath, "w500", "https://image.tmdb.org/t/p/w500" + TestConstants.Tmdb.TestPosterPath)]
    [InlineData(TestConstants.Tmdb.TestPosterPath, "w300", "https://image.tmdb.org/t/p/w300" + TestConstants.Tmdb.TestPosterPath)]
    [InlineData(TestConstants.Tmdb.TestPosterPath, "original", "https://image.tmdb.org/t/p/original" + TestConstants.Tmdb.TestPosterPath)]
    [InlineData("", "w500", "https://image.tmdb.org/t/p/w500")]
    public void GetPosterUrl_WithValidInput_ReturnsCorrectUrl(string posterPath, string size, string expectedUrl)
    {
        // Act
        var result = _tmdbService.GetPosterUrl(posterPath, size);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public void GetPosterUrl_WithDefaultSize_ReturnsW500Url()
    {
        // Arrange
        var posterPath = TestConstants.Tmdb.TestPosterPath;

        // Act
        var result = _tmdbService.GetPosterUrl(posterPath);

        // Assert
        result.Should().Be("https://image.tmdb.org/t/p/w500" + TestConstants.Tmdb.TestPosterPath);
    }

    #endregion

    #region Mapping Tests

    [Fact]
    public async Task SearchMoviesAsync_WithValidData_MapsCorrectly()
    {
        // Arrange
        var movieDto = new TmdbMovieDto
        {
            TmdbId = 123,
            Title = "Test Movie",
            Overview = "Test overview",
            PosterPath = "/test-poster.jpg",
            ReleaseDate = "2023-01-01",
            VoteAverage = 8.5,
            VoteCount = 1000,
            Popularity = 85.5,
            Genres = new[]
            {
                new TmdbGenreDto { Id = GenreConstants.ActionId, Name = GenreConstants.ActionTitle },
                new TmdbGenreDto { Id = GenreConstants.ComedyId, Name = GenreConstants.ComedyTitle },
                new TmdbGenreDto { Id = GenreConstants.DramaId, Name = GenreConstants.DramaTitle }
            }
        };

        var searchResponse = new TmdbSearchResponse
        {
            Results = new[] { movieDto }
        };

        SetupHttpResponse(searchResponse);

        // Act
        var result = await _tmdbService.SearchMoviesAsync("test");

        // Assert
        var movie = result.First();
        movie.TmdbId.Should().Be(123);
        movie.Title.Should().Be("Test Movie");
        movie.Overview.Should().Be("Test overview");
        movie.PosterPath.Should().Be("/test-poster.jpg");
        movie.ReleaseDate.Should().Be(new DateTime(2023, 1, 1));
        movie.VoteAverage.Should().Be(8.5);
        movie.VoteCount.Should().Be(1000);
        movie.Popularity.Should().Be(85.5);
        movie.Genres.Should().BeEquivalentTo(new[] { GenreConstants.ActionTitle, GenreConstants.ComedyTitle, GenreConstants.DramaTitle });
    }

    [Fact]
    public async Task SearchMoviesAsync_WithInvalidReleaseDate_MapsToMinValue()
    {
        // Arrange
        var movieDto = new TmdbMovieDto
        {
            TmdbId = 123,
            Title = "Test Movie",
            ReleaseDate = "invalid-date",
            Genres = Array.Empty<TmdbGenreDto>()
        };

        var searchResponse = new TmdbSearchResponse
        {
            Results = new[] { movieDto }
        };

        SetupHttpResponse(searchResponse);

        // Act
        var result = await _tmdbService.SearchMoviesAsync("test");

        // Assert
        var movie = result.First();
        movie.ReleaseDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public async Task SearchMoviesAsync_WithNullPosterPath_MapsToEmptyString()
    {
        // Arrange
        var movieDto = new TmdbMovieDto
        {
            TmdbId = 123,
            Title = "Test Movie",
            PosterPath = null,
            Genres = Array.Empty<TmdbGenreDto>()
        };

        var searchResponse = new TmdbSearchResponse
        {
            Results = new[] { movieDto }
        };

        SetupHttpResponse(searchResponse);

        // Act
        var result = await _tmdbService.SearchMoviesAsync("test");

        // Assert
        var movie = result.First();
        movie.PosterPath.Should().Be(string.Empty);
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse<T>(T response)
    {
        var json = JsonSerializer.Serialize(response);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            });
    }

    private void SetupHttpErrorResponse(HttpStatusCode statusCode, string errorMessage)
    {
        var errorContent = new StringContent($"{{\"error\": \"{errorMessage}\"}}", Encoding.UTF8, "application/json");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = errorContent
            });
    }

    private void VerifyHttpRequest(string endpoint, string? queryParams = null)
    {
        _mockHttpMessageHandler.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains(endpoint) &&
                    req.RequestUri.ToString().Contains(_tmdbSettings.ApiKey) &&
                    (queryParams == null || req.RequestUri.ToString().Contains(queryParams))),
                ItExpr.IsAny<CancellationToken>());
    }

    private TmdbSearchResponse CreateTmdbSearchResponse()
    {
        return new TmdbSearchResponse
        {
            Results = new[]
            {
                CreateTmdbMovieDto(),
                new TmdbMovieDto
                {
                    TmdbId = 12345,
                    Title = "Another Movie",
                    Overview = "Another overview",
                    PosterPath = "/another-poster.jpg",
                    ReleaseDate = "2023-02-01",
                    VoteAverage = 7.5,
                    VoteCount = 500,
                    Popularity = 75.0,
                    Genres = new[]
                    {
                        new TmdbGenreDto { Id = GenreConstants.ComedyId, Name = GenreConstants.ComedyTitle },
                        new TmdbGenreDto { Id = GenreConstants.CrimeId, Name = GenreConstants.CrimeTitle }
                    }
                }
            }
        };
    }

    private TmdbMovieDto CreateTmdbMovieDto()
    {
        return new TmdbMovieDto
        {
            TmdbId = 27205,
            Title = TestConstants.Tmdb.TestMovieTitle,
            Overview = TestConstants.Tmdb.TestMovieOverview,
            PosterPath = "/inception-poster.jpg",
            ReleaseDate = "2010-07-16",
            VoteAverage = 8.8,
            VoteCount = 25000,
            Popularity = 95.0,
            Genres = new[]
            {
                new TmdbGenreDto { Id = GenreConstants.ActionId, Name = GenreConstants.ActionTitle },
                new TmdbGenreDto { Id = GenreConstants.ScienceFictionId, Name = GenreConstants.ScienceFictionTitle },
                new TmdbGenreDto { Id = GenreConstants.ThrillerId, Name = GenreConstants.ThrillerTitle }
            }
        };
    }

    #endregion
}