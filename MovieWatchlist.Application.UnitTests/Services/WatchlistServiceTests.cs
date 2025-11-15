using Moq;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;
using MovieWatchlist.Application.Features.Watchlist.Commands.RemoveFromWatchlist;
using MovieWatchlist.Application.Features.Watchlist.Commands.UpdateWatchlistItem;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyFavoriteMovies;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyRecommendedMovies;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyStatistics;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlist;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByGenre;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByRatingRange;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByStatus;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByYearRange;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistItemById;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Application.UnitTests.Services;

public class WatchlistServiceTests
{
    private readonly Mock<IWatchlistRepository> _mockWatchlistRepository;
    private readonly Mock<IMovieRepository> _mockMovieRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITmdbService> _mockTmdbService;
    private readonly Mock<IRetryPolicyService> _mockRetryPolicyService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly WatchlistService _service;

    public WatchlistServiceTests()
    {
        _mockWatchlistRepository = new Mock<IWatchlistRepository>();
        _mockMovieRepository = new Mock<IMovieRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTmdbService = new Mock<ITmdbService>();
        _mockRetryPolicyService = new Mock<IRetryPolicyService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        // Setup default retry policy behavior
        _mockRetryPolicyService
            .Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<Task<Movie?>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns<Func<Task<Movie?>>, int, int, string>((func, maxRetries, baseDelayMs, operationName) => func());
        
        // Setup default current user service to return user ID 1
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(1);
        
        _service = new WatchlistService(
            _mockWatchlistRepository.Object, 
            _mockMovieRepository.Object,
            _mockUserRepository.Object,
            _mockTmdbService.Object,
            _mockRetryPolicyService.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetUserStatistics_WithMixedData_ReturnsCorrectStatistics()
    {
        // Arrange
        var testWatchlist = CreateTestWatchlist().ToList();
        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(testWatchlist);

        // Act
        var result = await _service.GetUserStatisticsAsync(new GetMyStatisticsQuery());

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
        var result = await _service.GetRecommendedMoviesAsync(new GetMyRecommendedMoviesQuery(Limit: 5));

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
        var result = await _service.GetWatchlistByGenreAsync(new GetMyWatchlistByGenreQuery(Genre: GenreConstants.Action));

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
        var result = await _service.GetWatchlistByYearRangeAsync(new GetMyWatchlistByYearRangeQuery(StartYear: 2020, EndYear: 2022));

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

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var result = await _service.GetFavoriteMoviesAsync(new GetMyFavoriteMoviesQuery());

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
        var movie1 = Movie()
            .WithId(1)
            .WithTitle(GenreConstants.ActionTitle)
            .WithVoteAverage(TestConstants.Ratings.HighRating)
            .WithVoteCount(5000)
            .WithReleaseDate(new DateTime(2020, 1, 1))
            .WithGenres(GenreConstants.Action, GenreConstants.Adventure)
            .Build();

        var movie2 = Movie()
            .WithId(2)
            .WithTitle("Action Movie 2")
            .WithVoteAverage(TestConstants.Ratings.DefaultTmdbRating)
            .WithVoteCount(3000)
            .WithReleaseDate(new DateTime(2021, 1, 1))
            .WithGenres(GenreConstants.Action, GenreConstants.Thriller)
            .Build();

        var movie3 = Movie()
            .WithId(3)
            .WithTitle(GenreConstants.DramaTitle)
            .WithVoteAverage(TestConstants.Ratings.HighRating)
            .WithVoteCount(2000)
            .WithReleaseDate(new DateTime(2022, 1, 1))
            .WithGenres(GenreConstants.Drama)
            .Build();

        var movie4 = Movie()
            .WithId(4)
            .WithTitle(GenreConstants.ComedyTitle)
            .WithVoteAverage(TestConstants.Ratings.LowRating)
            .WithVoteCount(1500)
            .WithReleaseDate(new DateTime(2019, 1, 1))
            .WithGenres(GenreConstants.Comedy)
            .Build();

        // Create watchlist items using factory methods
        var item1 = WatchlistItem().WithMovie(movie1).Build();
        item1.UpdateStatus(WatchlistStatus.Watched);
        item1.ToggleFavorite();
        item1.SetRating(Rating.Create(5).Value!);
        SetAddedDateViaReflection(item1, DateTime.UtcNow.AddDays(-10)); // Most recent

        var item2 = WatchlistItem().WithMovie(movie2).Build();
        item2.UpdateStatus(WatchlistStatus.Watched);
        item2.SetRating(Rating.Create(4).Value!);
        SetAddedDateViaReflection(item2, new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc)); // Previous year

        var item3 = WatchlistItem().WithMovie(movie3).Build();
        item3.ToggleFavorite();
        SetAddedDateViaReflection(item3, new DateTime(2021, 3, 10, 0, 0, 0, DateTimeKind.Utc)); // Two years ago

        var item4 = WatchlistItem().WithMovie(movie4).Build();
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
            Movie()
                .WithTitle("Recommended Action")
                .WithVoteAverage(TestConstants.Ratings.HighRating)
                .WithVoteCount(8000)
                .WithPopularity(1000.0)
                .WithGenres(GenreConstants.Action, GenreConstants.Adventure)
                .Build(),
            Movie()
                .WithTitle("Recommended Drama")
                .WithVoteAverage(TestConstants.Ratings.DefaultTmdbRating)
                .WithVoteCount(5000)
                .WithPopularity(800.0)
                .WithGenres(GenreConstants.Drama)
                .Build()
        };
    }

    #region CRUD Operation Tests

    [Fact]
    public async Task AddToWatchlistAsync_WithValidMovie_ReturnsWatchlistItem()
    {
        // Arrange
        var movie = Movie().WithTitle(GenreConstants.ActionTitle).Build();
        var command = new AddToWatchlistCommand(
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

        var testUser = User().Build();
        
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
        var movie = Movie().WithTitle(GenreConstants.ActionTitle).Build();

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

        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var command = new AddToWatchlistCommand(
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
        var command = new AddToWatchlistCommand(MovieId: 999);
        var result = await _service.AddToWatchlistAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(string.Format(ErrorMessages.MovieWithTmdbIdNotFound, 999), result.Error);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithDuplicateMovie_ReturnsFailure()
    {
        // Arrange
        var movie = Movie().WithTitle(GenreConstants.ActionTitle).Build();
        var existingWatchlist = new List<WatchlistItem>
        {
            WatchlistItem().Build()
        };

        // Mock movie in cache (return existing movie)
        _mockMovieRepository
            .Setup(x => x.GetByTmdbIdAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        // Mock watchlist to return existing item
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndTmdbIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(existingWatchlist.First());

        // Act
        var command = new AddToWatchlistCommand(MovieId: 1);
        var result = await _service.AddToWatchlistAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessages.MovieAlreadyInWatchlist, result.Error);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithValidItem_ReturnsUpdatedItem()
    {
        // Arrange
        var movie = Movie().Build();
        var existingItem = WatchlistItem().WithMovie(movie).Build();
        existingItem.UpdateStatus(WatchlistStatus.Planned);
        
        // Set Id using reflection for testing purposes
        typeof(WatchlistItem).GetProperty("Id")!.SetValue(existingItem, 1);

        var command = new UpdateWatchlistItemCommand(
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

        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
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
        _mockUserRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWatchlistItemAsync_WithNonExistentItem_ReturnsFailure()
    {
        // Arrange
        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        var command = new UpdateWatchlistItemCommand(
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
        var existingItem = WatchlistItem().Build();
        typeof(WatchlistItem).GetProperty("Id")!.SetValue(existingItem, TestConstants.WatchlistItems.FirstItemId);

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        var command = new UpdateWatchlistItemCommand(
            WatchlistItemId: TestConstants.WatchlistItems.FirstItemId,
            Status: WatchlistStatus.Watched
        );

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.FirstItemId))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
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
        var movie = Movie().Build();
        var existingItem = WatchlistItem().Build();
        typeof(WatchlistItem).GetProperty("Id")!.SetValue(existingItem, 1);
        existingItem.Movie = movie;

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(existingItem);

        _mockWatchlistRepository
            .Setup(x => x.DeleteAsync(It.IsAny<WatchlistItem>()))
            .Returns(Task.CompletedTask);

        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var command = new RemoveFromWatchlistCommand(WatchlistItemId: 1);
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
        var command = new RemoveFromWatchlistCommand(WatchlistItemId: 999);
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
        var existingItem = WatchlistItem().Build();
        typeof(WatchlistItem).GetProperty("Id")!.SetValue(existingItem, TestConstants.WatchlistItems.FirstItemId);

        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAndIdAsync(TestConstants.Users.DefaultUserId, TestConstants.WatchlistItems.FirstItemId))
            .ReturnsAsync(existingItem);

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyWatchlistItemByIdQuery(WatchlistItemId: TestConstants.WatchlistItems.FirstItemId);
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

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyWatchlistItemByIdQuery(WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId);
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

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyWatchlistQuery();
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

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyWatchlistByStatusQuery(Status: WatchlistStatus.Watched);
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

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyWatchlistByRatingRangeQuery(
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
        var testUser = User().Build();
        
        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(Enumerable.Empty<WatchlistItem>());

        // Act
        var query = new GetMyStatisticsQuery();
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
        var testUser = User().Build();
        
        var movie = new Movie { VoteAverage = TestConstants.Ratings.DefaultTmdbRating };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 1);
        var item = WatchlistItem().WithMovie(movie).Build();
        item.UpdateStatus(WatchlistStatus.Watched);
        var watchlistWithNoRatings = new List<WatchlistItem> { item };

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(testUser);
        
        _mockWatchlistRepository
            .Setup(x => x.GetByUserIdAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(watchlistWithNoRatings);

        // Setup current user service for this test
        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(TestConstants.Users.DefaultUserId);

        // Act
        var query = new GetMyStatisticsQuery();
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
        var query = new GetMyRecommendedMoviesQuery(Limit: 5);
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
        var query = new GetMyWatchlistByGenreQuery(Genre: "NonExistentGenre");
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
        var query = new GetMyWatchlistByYearRangeQuery(StartYear: 2030, EndYear: 2040);
        var result = await _service.GetWatchlistByYearRangeAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFavoriteMoviesAsync_WithNoFavorites_ReturnsEmpty()
    {
        // Arrange
        var movie = new Movie { VoteAverage = TestConstants.Ratings.DefaultTmdbRating };
        typeof(Movie).GetProperty("Id")!.SetValue(movie, 1);
        var item = WatchlistItem().WithMovie(movie).Build();
        var watchlistWithNoFavorites = new List<WatchlistItem> { item };

        _mockWatchlistRepository
            .Setup(x => x.GetFavoritesByUserIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(watchlistWithNoFavorites.Where(w => w.IsFavorite));

        // Act
        var query = new GetMyFavoriteMoviesQuery();
        var result = await _service.GetFavoriteMoviesAsync(query);

        // Assert
        Assert.Empty(result);
    }

    #endregion
} 