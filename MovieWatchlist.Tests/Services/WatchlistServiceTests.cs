using System.Linq.Expressions;
using Moq;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.DTOs;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Tests.Infrastructure;

namespace MovieWatchlist.Tests.Services;

public class WatchlistServiceTests : UnitTestBase
{
    private readonly Mock<IWatchlistRepository> _mockWatchlistRepository;
    private readonly Mock<IRepository<Movie>> _mockMovieRepository;
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly WatchlistService _service;

    public WatchlistServiceTests()
    {
        _mockWatchlistRepository = new Mock<IWatchlistRepository>();
        _mockMovieRepository = new Mock<IRepository<Movie>>();
        _mockTmdbService = new Mock<ITmdbService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new WatchlistService(
            _mockWatchlistRepository.Object, 
            _mockMovieRepository.Object,
            _mockTmdbService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetUserStatistics_WithMixedData_ReturnsCorrectStatistics()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().ToList();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetUserStatisticsAsync(1);

        // Assert
        Assert.Equal(4, result.TotalMovies);
        Assert.Equal(2, result.WatchedMovies);
        Assert.Equal(1, result.PlannedMovies);
        Assert.Equal(2, result.FavoriteMovies);
        Assert.Equal(1, result.MoviesThisYear);
        Assert.Equal(4.5, result.AverageUserRating);
        Assert.Equal(7.125, result.AverageTmdbRating);
        Assert.Equal(GenreConstants.Action, result.MostWatchedGenre);
        Assert.Equal(2, result.GenreBreakdown[GenreConstants.Action]);
        Assert.Equal(1, result.GenreBreakdown[GenreConstants.Adventure]);
        Assert.Equal(1, result.GenreBreakdown[GenreConstants.Thriller]);
    }

    [Fact]
    public async Task GetRecommendedMovies_WithUserPreferences_ReturnsFilteredMovies()
    {
        // Arrange
        var userWatchlist = CreateTestWatchlist();
        var allMovies = CreateTestMovies();

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(userWatchlist);

        _mockMovieRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(allMovies);

        // Act
        var result = await _service.GetRecommendedMoviesAsync(1, 5);

        // Assert
        var recommendations = result.ToList();
        Assert.True(recommendations.Count <= 5);
        
        // Verify recommendations are not in user's watchlist
        Assert.All(recommendations, movie => 
            Assert.DoesNotContain(userWatchlist, w => w.MovieId == movie.Id));
        
        // Verify recommendations have high ratings
        Assert.All(recommendations, movie => 
            Assert.True(movie.VoteAverage >= TestConstants.Ratings.DefaultTmdbRating));
        
        // Verify recommendations have sufficient votes
        Assert.All(recommendations, movie => 
            Assert.True(movie.VoteCount >= 1000));
    }

    [Fact]
    public async Task GetWatchlistByGenre_WithValidGenre_ReturnsFilteredResults()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().ToList();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByGenreAsync(1, GenreConstants.Action);

        // Assert
        var actionMovies = result.ToList();
        Assert.Equal(2, actionMovies.Count);
        Assert.All(actionMovies, item => 
            Assert.Contains(GenreConstants.Action, item.Movie.Genres));
    }

    [Fact]
    public async Task GetWatchlistByYearRange_WithValidRange_ReturnsFilteredResults()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByYearRangeAsync(1, 2020, 2022);

        // Assert
        var moviesInRange = result.ToList();
        Assert.Equal(3, moviesInRange.Count);
        Assert.All(moviesInRange, item => 
            Assert.True(item.Movie.ReleaseDate.Year >= 2020 && item.Movie.ReleaseDate.Year <= 2022));
    }

    [Fact]
    public async Task GetFavoriteMovies_WithFavorites_ReturnsOrderedResults()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().Where(w => w.IsFavorite).ToList();
        _mockWatchlistRepository
            .Setup(x => x.GetFavoritesByUserIdAsync(TestConstants.Users.DefaultUserId, It.IsAny<int>()))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetFavoriteMoviesAsync(TestConstants.Users.DefaultUserId);

        // Assert
        var favorites = result.ToList();
        const int expectedFavoriteCount = 2;
        Assert.Equal(expectedFavoriteCount, favorites.Count);
        Assert.All(favorites, item => Assert.True(item.IsFavorite));
        
        // Verify ordering by AddedDate (most recent first) - this is how GetFavoritesByUserIdAsync orders results
        Assert.True(favorites[TestConstants.CollectionIndices.First].AddedDate >= 
                    favorites[TestConstants.CollectionIndices.Second].AddedDate);
    }

    private static IEnumerable<WatchlistItem> CreateTestWatchlist()
    {
        return new List<WatchlistItem>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                MovieId = 1,
                Status = WatchlistStatus.Watched,
                IsFavorite = true,
                UserRating = 5,
                AddedDate = DateTime.UtcNow.AddDays(-10), // Current year
                Movie = new Movie
                {
                    Id = 1,
                    Title = GenreConstants.ActionTitle,
                    VoteAverage = TestConstants.Ratings.HighRating,
                    VoteCount = 5000,
                    ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-2),
                    Genres = new[] { GenreConstants.Action, GenreConstants.Adventure }
                }
            },
            new()
            {
                Id = 2,
                UserId = 1,
                MovieId = 2,
                Status = WatchlistStatus.Watched,
                IsFavorite = false,
                UserRating = 4,
                AddedDate = new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc), // Previous year
                Movie = new Movie
                {
                    Id = 2,
                    Title = "Action Movie 2",
                    VoteAverage = TestConstants.Ratings.DefaultTmdbRating,
                    VoteCount = 3000,
                    ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-1),
                    Genres = new[] { GenreConstants.Action, GenreConstants.Thriller }
                }
            },
            new()
            {
                Id = 3,
                UserId = 1,
                MovieId = 3,
                Status = WatchlistStatus.Planned,
                IsFavorite = true,
                UserRating = null,
                AddedDate = new DateTime(2021, 3, 10, 0, 0, 0, DateTimeKind.Utc), // Two years ago
                Movie = new Movie
                {
                    Id = 3,
                    Title = GenreConstants.DramaTitle,
                    VoteAverage = TestConstants.Ratings.HighRating,
                    VoteCount = 2000,
                    ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-3),
                    Genres = new[] { GenreConstants.Drama }
                }
            },
            new()
            {
                Id = 4,
                UserId = 1,
                MovieId = 4,
                Status = WatchlistStatus.Watching,
                IsFavorite = false,
                UserRating = null,
                AddedDate = new DateTime(2020, 8, 20, 0, 0, 0, DateTimeKind.Utc), // Three years ago
                Movie = new Movie
                {
                    Id = 4,
                    Title = GenreConstants.ComedyTitle,
                    VoteAverage = TestConstants.Ratings.LowRating,
                    VoteCount = 1500,
                    ReleaseDate = TestConstants.Dates.DefaultReleaseDate,
                    Genres = new[] { GenreConstants.Comedy }
                }
            }
        };
    }

    private static IEnumerable<Movie> CreateTestMovies()
    {
        return new List<Movie>
        {
            new()
            {
                Id = 5,
                Title = "Recommended Action",
                VoteAverage = TestConstants.Ratings.HighRating,
                VoteCount = 8000,
                Popularity = 1000.0,
                Genres = new[] { GenreConstants.Action, GenreConstants.Adventure }
            },
            new()
            {
                Id = 6,
                Title = "Recommended Drama",
                VoteAverage = TestConstants.Ratings.DefaultTmdbRating,
                VoteCount = 5000,
                Popularity = 800.0,
                Genres = new[] { GenreConstants.Drama }
            }
        };
    }

    #region CRUD Operation Tests

    [Fact]
    public async Task AddToWatchlistAsync_WithValidMovie_ReturnsWatchlistItem()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: GenreConstants.ActionTitle);
        var addDto = new AddToWatchlistDto { Notes = "Test note" };

        // Mock movie not in cache (FindAsync returns empty)
        _mockMovieRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Movie>());

        // Mock TMDB service to return movie
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync(movie);

        _mockMovieRepository
            .Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        _mockWatchlistRepository
            .Setup(x => x.AddAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddToWatchlistAsync(1, 1, WatchlistStatus.Planned, addDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.MovieId);
        Assert.Equal(WatchlistStatus.Planned, result.Status);
        Assert.Equal("Test note", result.Notes);
        Assert.True(result.AddedDate != default);
        Assert.Null(result.WatchedDate);

        _mockWatchlistRepository.Verify(x => x.AddAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithWatchedStatus_SetsWatchedDate()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: GenreConstants.ActionTitle);

        // Mock movie not in cache
        _mockMovieRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Movie>());

        // Mock TMDB service to return movie
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync(movie);

        _mockMovieRepository
            .Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        _mockWatchlistRepository
            .Setup(x => x.AddAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddToWatchlistAsync(1, 1, WatchlistStatus.Watched);

        // Assert
        Assert.True(result.WatchedDate != default);
        Assert.Equal(WatchlistStatus.Watched, result.Status);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithNonExistentMovie_ThrowsArgumentException()
    {
        // Arrange
        // Mock movie not in cache
        _mockMovieRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Movie>());

        // Mock TMDB service to return null
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(999))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.AddToWatchlistAsync(1, 999));
        
        Assert.Contains("Movie with TMDB ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithDuplicateMovie_ThrowsInvalidOperationException()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: GenreConstants.ActionTitle);
        var existingWatchlist = new List<WatchlistItem>
        {
            CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1, status: WatchlistStatus.Planned)
        };

        // Mock movie in cache (return existing movie)
        _mockMovieRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Movie, bool>>>()))
            .ReturnsAsync(new List<Movie> { movie });

        // Mock watchlist to return existing item
        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(existingWatchlist);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.AddToWatchlistAsync(1, 1));
        
        Assert.Contains("Movie is already in user's watchlist", exception.Message);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithValidItem_ReturnsUpdatedItem()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1, status: WatchlistStatus.Planned);

        var updateDto = new UpdateWatchlistItemDto
        {
            Status = WatchlistStatus.Watched,
            IsFavorite = true,
            UserRating = 5,
            Notes = "Updated notes"
        };

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(1, 1))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(1, 1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WatchlistStatus.Watched, result.Status);
        Assert.True(result.IsFavorite);
        Assert.Equal(5, result.UserRating);
        Assert.Equal("Updated notes", result.Notes);
        Assert.True(result.WatchedDate != default);

        _mockWatchlistRepository.Verify(x => x.UpdateAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithNonExistentItem_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateWatchlistItemDto { Status = WatchlistStatus.Watched };

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.NonExistentItemId))
            .ReturnsAsync((WatchlistItem?)null);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(
            TestConstants.Users.DefaultUserId, 
            TestConstants.WatchlistItems.NonExistentItemId, 
            updateDto);

        // Assert
        Assert.Null(result);
        _mockWatchlistRepository.Verify(x => x.UpdateAsync(It.IsAny<WatchlistItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithWatchedStatus_SetsWatchedDate()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(
            id: TestConstants.WatchlistItems.FirstItemId, 
            userId: TestConstants.Users.DefaultUserId, 
            movieId: TestConstants.WatchlistItems.FirstItemId, 
            status: WatchlistStatus.Planned);

        var updateDto = new UpdateWatchlistItemDto
        {
            Status = WatchlistStatus.Watched
        };

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.FirstItemId))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(
            TestConstants.Users.DefaultUserId, 
            TestConstants.WatchlistItems.FirstItemId, 
            updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WatchlistStatus.Watched, result.Status);
        Assert.True(result.WatchedDate != default);
    }

    [Fact]
    public async Task RemoveFromWatchlistAsync_WithValidItem_ReturnsTrue()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1);

        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(new List<WatchlistItem> { existingItem });

        _mockWatchlistRepository
            .Setup(x => x.DeleteAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveFromWatchlistAsync(1, 1);

        // Assert
        Assert.True(result);
        _mockWatchlistRepository.Verify(x => x.DeleteAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromWatchlistAsync_WithNonExistentItem_ReturnsFalse()
    {
        // Arrange
        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        // Act
        var result = await _service.RemoveFromWatchlistAsync(1, 999);

        // Assert
        Assert.False(result);
        _mockWatchlistRepository.Verify(x => x.DeleteAsync(It.IsAny<WatchlistItem>()), Times.Never);
    }

    [Fact]
    public async Task GetWatchlistItemByIdAsync_WithValidId_ReturnsItem()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(
            id: TestConstants.WatchlistItems.FirstItemId, 
            userId: TestConstants.Users.DefaultUserId, 
            movieId: TestConstants.WatchlistItems.FirstItemId);

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.FirstItemId))
            .ReturnsAsync(existingItem);

        // Act
        var result = await _service.GetWatchlistItemByIdAsync(
            TestConstants.Users.DefaultUserId, 
            TestConstants.WatchlistItems.FirstItemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestConstants.WatchlistItems.FirstItemId, result.Id);
        Assert.Equal(TestConstants.Users.DefaultUserId, result.UserId);
    }

    [Fact]
    public async Task GetWatchlistItemByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.NonExistentItemId))
            .ReturnsAsync((WatchlistItem?)null);

        // Act
        var result = await _service.GetWatchlistItemByIdAsync(
            TestConstants.Users.DefaultUserId, 
            TestConstants.WatchlistItems.NonExistentItemId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Missing Method Tests

    [Fact]
    public async Task GetUserWatchlistAsync_ReturnsOrderedByAddedDate()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().ToList();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetUserWatchlistAsync(TestConstants.Users.DefaultUserId);

        // Assert
        var watchlist = result.ToList();
        const int expectedWatchlistCount = 4;
        Assert.Equal(expectedWatchlistCount, watchlist.Count);
        
        // Verify ordering by AddedDate (most recent first)
        Assert.True(watchlist[TestConstants.CollectionIndices.First].AddedDate >= 
                    watchlist[TestConstants.CollectionIndices.Second].AddedDate);
        Assert.True(watchlist[TestConstants.CollectionIndices.Second].AddedDate >= 
                    watchlist[TestConstants.CollectionIndices.Third].AddedDate);
        Assert.True(watchlist[TestConstants.CollectionIndices.Third].AddedDate >= 
                    watchlist[TestConstants.CollectionIndices.Fourth].AddedDate);
    }

    [Fact]
    public async Task GetWatchlistByStatusAsync_WithWatchedStatus_ReturnsOnlyWatched()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().Where(x => x.Status == WatchlistStatus.Watched).ToList();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndStatusAsync(TestConstants.Users.DefaultUserId, WatchlistStatus.Watched))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByStatusAsync(TestConstants.Users.DefaultUserId, WatchlistStatus.Watched);

        // Assert
        var watchedItems = result.ToList();
        const int expectedWatchedCount = 2;
        Assert.Equal(expectedWatchedCount, watchedItems.Count);
        Assert.All(watchedItems, item => Assert.Equal(WatchlistStatus.Watched, item.Status));
    }

    [Fact]
    public async Task GetWatchlistByRatingRangeAsync_WithValidRange_ReturnsFilteredResults()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByRatingRangeAsync(
            TestConstants.Users.DefaultUserId, 
            TestConstants.Ratings.DefaultTmdbRating, 
            TestConstants.Ratings.HighRating);

        // Assert
        var moviesInRange = result.ToList();
        const int expectedMoviesInRange = 3;
        Assert.Equal(expectedMoviesInRange, moviesInRange.Count);
        Assert.All(moviesInRange, item => 
            Assert.True(item.Movie.VoteAverage >= TestConstants.Ratings.DefaultTmdbRating && 
                       item.Movie.VoteAverage <= TestConstants.Ratings.HighRating));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetUserStatisticsAsync_WithEmptyWatchlist_ReturnsZeroStatistics()
    {
        // Arrange
        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        // Act
        var result = await _service.GetUserStatisticsAsync(1);

        // Assert
        Assert.Equal(0, result.TotalMovies);
        Assert.Equal(0, result.WatchedMovies);
        Assert.Equal(0, result.PlannedMovies);
        Assert.Equal(0, result.FavoriteMovies);
        Assert.Equal(0, result.MoviesThisYear);
        Assert.Equal(0.0, result.AverageUserRating);
        Assert.Equal(0.0, result.AverageTmdbRating);
        Assert.Equal(string.Empty, result.MostWatchedGenre);
        Assert.Empty(result.GenreBreakdown);
        Assert.Empty(result.YearlyBreakdown);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithNoUserRatings_ReturnsNullAverageUserRating()
    {
        // Arrange
        var watchlistWithNoRatings = new List<WatchlistItem>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                MovieId = 1,
                Status = WatchlistStatus.Watched,
                UserRating = null,
                Movie = new Movie { VoteAverage = TestConstants.Ratings.DefaultTmdbRating }
            }
        };

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(watchlistWithNoRatings);

        // Act
        var result = await _service.GetUserStatisticsAsync(TestConstants.Users.DefaultUserId);

        // Assert
        Assert.Equal(0.0, result.AverageUserRating);
        Assert.Equal(TestConstants.Ratings.DefaultTmdbRating, result.AverageTmdbRating);
    }

    [Fact]
    public async Task GetRecommendedMoviesAsync_WithNoUserPreferences_ReturnsEmpty()
    {
        // Arrange
        var emptyWatchlist = Enumerable.Empty<WatchlistItem>();
        var allMovies = CreateTestMovies();

        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(emptyWatchlist);

        _mockMovieRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(allMovies);

        // Act
        var result = await _service.GetRecommendedMoviesAsync(1, 5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWatchlistByGenreAsync_WithNonExistentGenre_ReturnsEmpty()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByGenreAsync(1, "NonExistentGenre");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWatchlistByYearRangeAsync_WithInvalidRange_ReturnsEmpty()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetWatchlistByYearRangeAsync(1, 2030, 2040);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFavoriteMoviesAsync_WithNoFavorites_ReturnsEmpty()
    {
        // Arrange
        var watchlistWithNoFavorites = new List<WatchlistItem>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                MovieId = 1,
                IsFavorite = false,
                Movie = new Movie { VoteAverage = TestConstants.Ratings.DefaultTmdbRating }
            }
        };

        _mockWatchlistRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WatchlistItem, bool>>>()))
            .ReturnsAsync((Expression<Func<WatchlistItem, bool>> predicate) => 
                watchlistWithNoFavorites.Where(predicate.Compile()));

        // Act
        var result = await _service.GetFavoriteMoviesAsync(1);

        // Assert
        Assert.Empty(result);
    }

    #endregion
} 