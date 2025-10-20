using Moq;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Tests.Infrastructure;

namespace MovieWatchlist.Tests.Services;

public class WatchlistServiceTests : UnitTestBase
{
    private readonly Mock<IWatchlistRepository> _mockWatchlistRepository;
    private readonly Mock<IMovieRepository> _mockMovieRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly WatchlistService _service;

    public WatchlistServiceTests()
    {
        _mockWatchlistRepository = new Mock<IWatchlistRepository>();
        _mockMovieRepository = new Mock<IMovieRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTmdbService = new Mock<ITmdbService>();
        _service = new WatchlistService(
            _mockWatchlistRepository.Object, 
            _mockMovieRepository.Object,
            _mockUserRepository.Object,
            _mockTmdbService.Object);
    }

    [Fact]
    public async Task GetUserStatistics_WithMixedData_ReturnsCorrectStatistics()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().ToList();
        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetUserStatisticsAsync(new GetUserStatisticsQuery(UserId: 1));

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
        var result = await _service.GetRecommendedMoviesAsync(new GetRecommendedMoviesQuery(UserId: 1, Limit: 5));

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
        var result = await _service.GetWatchlistByGenreAsync(new GetWatchlistByGenreQuery(UserId: 1, Genre: GenreConstants.Action));

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
        var result = await _service.GetWatchlistByYearRangeAsync(new GetWatchlistByYearRangeQuery(UserId: 1, StartYear: 2020, EndYear: 2022));

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
        var result = await _service.GetFavoriteMoviesAsync(new GetFavoriteMoviesQuery(UserId: TestConstants.Users.DefaultUserId));

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
        var movie1 = new Movie
        {
            Id = 1,
            Title = GenreConstants.ActionTitle,
            VoteAverage = TestConstants.Ratings.HighRating,
            VoteCount = 5000,
            ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-2),
            Genres = new[] { GenreConstants.Action, GenreConstants.Adventure }
        };

        var movie2 = new Movie
        {
            Id = 2,
            Title = "Action Movie 2",
            VoteAverage = TestConstants.Ratings.DefaultTmdbRating,
            VoteCount = 3000,
            ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-1),
            Genres = new[] { GenreConstants.Action, GenreConstants.Thriller }
        };

        var movie3 = new Movie
        {
            Id = 3,
            Title = GenreConstants.DramaTitle,
            VoteAverage = TestConstants.Ratings.HighRating,
            VoteCount = 2000,
            ReleaseDate = TestConstants.Dates.DefaultReleaseDate.AddYears(-3),
            Genres = new[] { GenreConstants.Drama }
        };

        var movie4 = new Movie
        {
            Id = 4,
            Title = GenreConstants.ComedyTitle,
            VoteAverage = TestConstants.Ratings.LowRating,
            VoteCount = 1500,
            ReleaseDate = TestConstants.Dates.DefaultReleaseDate,
            Genres = new[] { GenreConstants.Comedy }
        };

        // Create watchlist items using factory methods
        var item1 = WatchlistItem.Create(1, 1, movie1);
        item1.UpdateStatus(WatchlistStatus.Watched);
        item1.ToggleFavorite();
        item1.SetRating(Rating.Create(5).Value!);
        SetAddedDateViaReflection(item1, DateTime.UtcNow.AddDays(-10)); // Most recent

        var item2 = WatchlistItem.Create(1, 2, movie2);
        item2.UpdateStatus(WatchlistStatus.Watched);
        item2.SetRating(Rating.Create(4).Value!);
        SetAddedDateViaReflection(item2, new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc)); // Previous year

        var item3 = WatchlistItem.Create(1, 3, movie3);
        item3.ToggleFavorite();
        SetAddedDateViaReflection(item3, new DateTime(2021, 3, 10, 0, 0, 0, DateTimeKind.Utc)); // Two years ago

        var item4 = WatchlistItem.Create(1, 4, movie4);
        item4.UpdateStatus(WatchlistStatus.Watching);
        SetAddedDateViaReflection(item4, new DateTime(2020, 8, 20, 0, 0, 0, DateTimeKind.Utc)); // Three years ago

        return new List<WatchlistItem> { item1, item2, item3, item4 };
    }

    private static void SetAddedDateViaReflection(WatchlistItem item, DateTime addedDate)
    {
        var property = typeof(WatchlistItem).GetProperty("AddedDate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(item, addedDate);
        }
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
        var command = new AddToWatchlistCommand(
            UserId: 1,
            MovieId: 1,
            Status: WatchlistStatus.Planned,
            Notes: "Test note"
        );

        // Mock movie not in cache (GetByTmdbIdAsync returns null)
        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Movie?)null);

        // Mock TMDB service to return movie
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync(movie);

        _mockMovieRepository
            .Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);


        _mockWatchlistRepository
            .Setup(x => x.IsMovieInUserWatchlistAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        _mockWatchlistRepository
            .Setup(x => x.AddAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddToWatchlistAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value!.UserId);
        Assert.Equal(1, result.Value.MovieId);
        Assert.Equal(WatchlistStatus.Planned, result.Value.Status);
        Assert.Equal("Test note", result.Value.Notes);
        Assert.True(result.Value.AddedDate != default);
        Assert.Null(result.Value.WatchedDate);

        _mockWatchlistRepository.Verify(x => x.AddAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithWatchedStatus_SetsWatchedDate()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: GenreConstants.ActionTitle);

        // Mock movie not in cache
        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Movie?)null);

        // Mock TMDB service to return movie
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(1))
            .ReturnsAsync(movie);

        _mockMovieRepository
            .Setup(x => x.AddAsync(It.IsAny<Movie>()))
            .Returns(Task.CompletedTask);


        _mockWatchlistRepository
            .Setup(x => x.IsMovieInUserWatchlistAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        _mockWatchlistRepository
            .Setup(x => x.AddAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var command = new AddToWatchlistCommand(
            UserId: 1,
            MovieId: 1,
            Status: WatchlistStatus.Watched
        );
        var result = await _service.AddToWatchlistAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.WatchedDate != default);
        Assert.Equal(WatchlistStatus.Watched, result.Value.Status);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithNonExistentMovie_ReturnsFailure()
    {
        // Arrange
        // Mock movie not in cache
        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Movie?)null);

        // Mock TMDB service to return null
        _mockTmdbService
            .Setup(x => x.GetMovieDetailsAsync(999))
            .ReturnsAsync((Movie?)null);

        // Act
        var command = new AddToWatchlistCommand(UserId: 1, MovieId: 999);
        var result = await _service.AddToWatchlistAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(string.Format(ErrorMessages.MovieNotFound, 999), result.Error);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithDuplicateMovie_ReturnsFailure()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: GenreConstants.ActionTitle);
        var existingWatchlist = new List<WatchlistItem>
        {
            CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1, status: WatchlistStatus.Planned)
        };

        // Mock movie in cache (return existing movie)
        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        // Mock watchlist to return existing item
        _mockWatchlistRepository
            .Setup(x => x.IsMovieInUserWatchlistAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var command = new AddToWatchlistCommand(UserId: 1, MovieId: 1);
        var result = await _service.AddToWatchlistAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessages.MovieAlreadyInWatchlist, result.Error);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithValidItem_ReturnsUpdatedItem()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1, status: WatchlistStatus.Planned);

        var command = new UpdateWatchlistItemCommand(
            UserId: 1,
            WatchlistItemId: 1,
            Status: WatchlistStatus.Watched,
            IsFavorite: true,
            UserRating: 5,
            Notes: "Updated notes"
        );

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(1, 1))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(WatchlistStatus.Watched, result.Value!.Status);
        Assert.True(result.Value.IsFavorite);
        Assert.Equal(5, result.Value.UserRating!.Value);
        Assert.Equal("Updated notes", result.Value.Notes);
        Assert.True(result.Value.WatchedDate != default);

        _mockWatchlistRepository.Verify(x => x.UpdateAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithNonExistentItem_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateWatchlistItemCommand(
            UserId: TestConstants.Users.DefaultUserId,
            WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId,
            Status: WatchlistStatus.Watched
        );

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.NonExistentItemId))
            .ReturnsAsync((WatchlistItem?)null);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessages.WatchlistItemNotFound, result.Error);
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

        var command = new UpdateWatchlistItemCommand(
            UserId: TestConstants.Users.DefaultUserId,
            WatchlistItemId: TestConstants.WatchlistItems.FirstItemId,
            Status: WatchlistStatus.Watched
        );

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.FirstItemId))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateWatchlistItemAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(WatchlistStatus.Watched, result.Value!.Status);
        Assert.True(result.Value.WatchedDate != default);
    }

    [Fact]
    public async Task RemoveFromWatchlistAsync_WithValidItem_ReturnsTrue()
    {
        // Arrange
        var existingItem = CreateTestWatchlistItem(id: 1, userId: 1, movieId: 1);

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.DeleteAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var command = new RemoveFromWatchlistCommand(UserId: 1, WatchlistItemId: 1);
        var result = await _service.RemoveFromWatchlistAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _mockWatchlistRepository.Verify(x => x.DeleteAsync(It.IsAny<WatchlistItem>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromWatchlistAsync_WithNonExistentItem_ReturnsFailure()
    {
        // Arrange
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((WatchlistItem?)null);

        // Act
        var command = new RemoveFromWatchlistCommand(UserId: 1, WatchlistItemId: 999);
        var result = await _service.RemoveFromWatchlistAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessages.WatchlistItemNotFound, result.Error);
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
        var query = new GetWatchlistItemByIdQuery(
            UserId: TestConstants.Users.DefaultUserId,
            WatchlistItemId: TestConstants.WatchlistItems.FirstItemId
        );
        var result = await _service.GetWatchlistItemByIdAsync(query);

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
        var query = new GetWatchlistItemByIdQuery(
            UserId: TestConstants.Users.DefaultUserId,
            WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId
        );
        var result = await _service.GetWatchlistItemByIdAsync(query);

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
        var query = new GetUserWatchlistQuery(UserId: TestConstants.Users.DefaultUserId);
        var result = await _service.GetUserWatchlistAsync(query);

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
        var query = new GetWatchlistByStatusQuery(UserId: TestConstants.Users.DefaultUserId, Status: WatchlistStatus.Watched);
        var result = await _service.GetWatchlistByStatusAsync(query);

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
        var query = new GetWatchlistByRatingRangeQuery(
            UserId: TestConstants.Users.DefaultUserId,
            MinRating: TestConstants.Ratings.DefaultTmdbRating,
            MaxRating: TestConstants.Ratings.HighRating
        );
        var result = await _service.GetWatchlistByRatingRangeAsync(query);

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
        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        // Act
        var query = new GetUserStatisticsQuery(UserId: 1);
        var result = await _service.GetUserStatisticsAsync(query);

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
        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashedpassword"
        );
        
        var movie = new Movie { Id = 1, VoteAverage = TestConstants.Ratings.DefaultTmdbRating };
        var item = WatchlistItem.Create(1, 1, movie);
        item.UpdateStatus(WatchlistStatus.Watched);
        var watchlistWithNoRatings = new List<WatchlistItem> { item };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(watchlistWithNoRatings);

        // Act
        var query = new GetUserStatisticsQuery(UserId: TestConstants.Users.DefaultUserId);
        var result = await _service.GetUserStatisticsAsync(query);

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
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(emptyWatchlist);

        _mockMovieRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(allMovies);

        // Act
        var query = new GetRecommendedMoviesQuery(UserId: 1, Limit: 5);
        var result = await _service.GetRecommendedMoviesAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWatchlistByGenreAsync_WithNonExistentGenre_ReturnsEmpty()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(testWatchlist);

        // Act
        var query = new GetWatchlistByGenreQuery(UserId: 1, Genre: "NonExistentGenre");
        var result = await _service.GetWatchlistByGenreAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWatchlistByYearRangeAsync_WithInvalidRange_ReturnsEmpty()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist();
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(testWatchlist);

        // Act
        var query = new GetWatchlistByYearRangeQuery(UserId: 1, StartYear: 2030, EndYear: 2040);
        var result = await _service.GetWatchlistByYearRangeAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFavoriteMoviesAsync_WithNoFavorites_ReturnsEmpty()
    {
        // Arrange
        var movie = new Movie { Id = 1, VoteAverage = TestConstants.Ratings.DefaultTmdbRating };
        var item = WatchlistItem.Create(1, 1, movie);
        var watchlistWithNoFavorites = new List<WatchlistItem> { item };

        _mockWatchlistRepository
            .Setup(x => x.GetFavoritesByUserIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(watchlistWithNoFavorites.Where(w => w.IsFavorite));

        // Act
        var query = new GetFavoriteMoviesQuery(UserId: 1);
        var result = await _service.GetFavoriteMoviesAsync(query);

        // Assert
        Assert.Empty(result);
    }

    #endregion
} 