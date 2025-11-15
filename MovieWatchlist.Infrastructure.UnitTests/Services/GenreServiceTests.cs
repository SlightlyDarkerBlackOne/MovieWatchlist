using FluentAssertions;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Infrastructure.Services;
using Xunit;

namespace MovieWatchlist.Infrastructure.UnitTests.Services;

/// <summary>
/// Unit tests for GenreService
/// </summary>
public class GenreServiceTests
{
    private readonly GenreService _genreService;

    public GenreServiceTests()
    {
        _genreService = new GenreService();
    }

    #region GetGenreId Tests

    [Theory]
    [InlineData(GenreConstants.Action, GenreConstants.ActionId)]
    [InlineData(GenreConstants.Adventure, GenreConstants.AdventureId)]
    [InlineData(GenreConstants.Animation, GenreConstants.AnimationId)]
    [InlineData(GenreConstants.Comedy, GenreConstants.ComedyId)]
    [InlineData(GenreConstants.Crime, GenreConstants.CrimeId)]
    [InlineData(GenreConstants.Documentary, GenreConstants.DocumentaryId)]
    [InlineData(GenreConstants.Drama, GenreConstants.DramaId)]
    [InlineData(GenreConstants.Family, GenreConstants.FamilyId)]
    [InlineData(GenreConstants.Fantasy, GenreConstants.FantasyId)]
    [InlineData(GenreConstants.History, GenreConstants.HistoryId)]
    [InlineData(GenreConstants.Horror, GenreConstants.HorrorId)]
    [InlineData(GenreConstants.Music, GenreConstants.MusicId)]
    [InlineData(GenreConstants.Mystery, GenreConstants.MysteryId)]
    [InlineData(GenreConstants.Romance, GenreConstants.RomanceId)]
    [InlineData(GenreConstants.ScienceFiction, GenreConstants.ScienceFictionId)]
    [InlineData(GenreConstants.SciFi, GenreConstants.SciFiId)]
    [InlineData(GenreConstants.TvMovie, GenreConstants.TvMovieId)]
    [InlineData(GenreConstants.Thriller, GenreConstants.ThrillerId)]
    [InlineData(GenreConstants.War, GenreConstants.WarId)]
    [InlineData(GenreConstants.Western, GenreConstants.WesternId)]
    public void GetGenreId_WithValidGenreNames_ReturnsCorrectId(string genreName, int expectedId)
    {
        // Act
        var result = _genreService.GetGenreId(genreName);

        // Assert
        result.Should().Be(expectedId);
    }

    [Theory]
    [InlineData(GenreConstants.Action)]
    [InlineData("ACTION")]
    [InlineData(GenreConstants.Comedy)]
    [InlineData("COMEDY")]
    [InlineData(GenreConstants.SciFi)]
    [InlineData("SCI-FI")]
    [InlineData(GenreConstants.ScienceFiction)]
    [InlineData("SCIENCE FICTION")]
    public void GetGenreId_WithCaseInsensitiveGenreNames_ReturnsCorrectId(string genreName)
    {
        // Act
        var result = _genreService.GetGenreId(genreName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BePositive();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("action movie")]
    [InlineData("superhero")]
    [InlineData("dramatic")]
    public void GetGenreId_WithInvalidGenreNames_ReturnsNull(string genreName)
    {
        // Act
        var result = _genreService.GetGenreId(genreName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetGenreId_WithNullGenreName_ReturnsNull()
    {
        // Act
        var result = _genreService.GetGenreId(null!);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetGenreName Tests

    [Theory]
    [InlineData(GenreConstants.ActionId, GenreConstants.Action)]
    [InlineData(GenreConstants.AdventureId, GenreConstants.Adventure)]
    [InlineData(GenreConstants.AnimationId, GenreConstants.Animation)]
    [InlineData(GenreConstants.ComedyId, GenreConstants.Comedy)]
    [InlineData(GenreConstants.CrimeId, GenreConstants.Crime)]
    [InlineData(GenreConstants.DocumentaryId, GenreConstants.Documentary)]
    [InlineData(GenreConstants.DramaId, GenreConstants.Drama)]
    [InlineData(GenreConstants.FamilyId, GenreConstants.Family)]
    [InlineData(GenreConstants.FantasyId, GenreConstants.Fantasy)]
    [InlineData(GenreConstants.HistoryId, GenreConstants.History)]
    [InlineData(GenreConstants.HorrorId, GenreConstants.Horror)]
    [InlineData(GenreConstants.MusicId, GenreConstants.Music)]
    [InlineData(GenreConstants.MysteryId, GenreConstants.Mystery)]
    [InlineData(GenreConstants.RomanceId, GenreConstants.Romance)]
    [InlineData(GenreConstants.ScienceFictionId, GenreConstants.ScienceFiction)]
    [InlineData(GenreConstants.TvMovieId, GenreConstants.TvMovie)]
    [InlineData(GenreConstants.ThrillerId, GenreConstants.Thriller)]
    [InlineData(GenreConstants.WarId, GenreConstants.War)]
    [InlineData(GenreConstants.WesternId, GenreConstants.Western)]
    public void GetGenreName_WithValidGenreIds_ReturnsCorrectName(int genreId, string expectedName)
    {
        // Act
        var result = _genreService.GetGenreName(genreId);

        // Assert
        result.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(99999)]
    public void GetGenreName_WithInvalidGenreIds_ReturnsNull(int genreId)
    {
        // Act
        var result = _genreService.GetGenreName(genreId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllGenres Tests

    [Fact]
    public void GetAllGenres_ReturnsAllGenres()
    {
        // Act
        var result = _genreService.GetAllGenres();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(20); // Total number of genres in the service
        result.Should().ContainKey(GenreConstants.Action).WhoseValue.Should().Be(GenreConstants.ActionId);
        result.Should().ContainKey(GenreConstants.Comedy).WhoseValue.Should().Be(GenreConstants.ComedyId);
        result.Should().ContainKey(GenreConstants.ScienceFiction).WhoseValue.Should().Be(GenreConstants.ScienceFictionId);
        result.Should().ContainKey(GenreConstants.SciFi).WhoseValue.Should().Be(GenreConstants.SciFiId);
    }

    [Fact]
    public void GetAllGenres_ReturnsReadOnlyCopy()
    {
        // Act
        var result1 = _genreService.GetAllGenres();
        var result2 = _genreService.GetAllGenres();

        // Assert
        result1.Should().NotBeSameAs(result2); // Should be different instances
        result1.Should().BeEquivalentTo(result2); // But with same content
    }

    [Fact]
    public void GetAllGenres_ContainsAllExpectedGenres()
    {
        // Arrange
        var expectedGenres = new Dictionary<string, int>
        {
            { GenreConstants.Action, GenreConstants.ActionId },
            { GenreConstants.Adventure, GenreConstants.AdventureId },
            { GenreConstants.Animation, GenreConstants.AnimationId },
            { GenreConstants.Comedy, GenreConstants.ComedyId },
            { GenreConstants.Crime, GenreConstants.CrimeId },
            { GenreConstants.Documentary, GenreConstants.DocumentaryId },
            { GenreConstants.Drama, GenreConstants.DramaId },
            { GenreConstants.Family, GenreConstants.FamilyId },
            { GenreConstants.Fantasy, GenreConstants.FantasyId },
            { GenreConstants.History, GenreConstants.HistoryId },
            { GenreConstants.Horror, GenreConstants.HorrorId },
            { GenreConstants.Music, GenreConstants.MusicId },
            { GenreConstants.Mystery, GenreConstants.MysteryId },
            { GenreConstants.Romance, GenreConstants.RomanceId },
            { GenreConstants.ScienceFiction, GenreConstants.ScienceFictionId },
            { GenreConstants.SciFi, GenreConstants.SciFiId },
            { GenreConstants.TvMovie, GenreConstants.TvMovieId },
            { GenreConstants.Thriller, GenreConstants.ThrillerId },
            { GenreConstants.War, GenreConstants.WarId },
            { GenreConstants.Western, GenreConstants.WesternId }
        };

        // Act
        var result = _genreService.GetAllGenres();

        // Assert
        result.Should().BeEquivalentTo(expectedGenres);
    }

    #endregion

    #region Round Trip Tests

    [Theory]
    [InlineData(GenreConstants.Action)]
    [InlineData(GenreConstants.Comedy)]
    [InlineData(GenreConstants.Drama)]
    [InlineData(GenreConstants.Horror)]
    [InlineData(GenreConstants.ScienceFiction)]
    public void GetGenreIdAndGetGenreName_RoundTrip_ReturnsOriginalValue(string genreName)
    {
        // Act
        var genreId = _genreService.GetGenreId(genreName);
        var resultGenreName = _genreService.GetGenreName(genreId!.Value);

        // Assert
        genreId.Should().NotBeNull();
        resultGenreName.Should().Be(genreName);
    }

    [Theory]
    [InlineData(GenreConstants.ActionId)]
    [InlineData(GenreConstants.ComedyId)]
    [InlineData(GenreConstants.DramaId)]
    [InlineData(GenreConstants.HorrorId)]
    [InlineData(GenreConstants.ScienceFictionId)]
    public void GetGenreNameAndGetGenreId_RoundTrip_ReturnsOriginalValue(int genreId)
    {
        // Act
        var genreName = _genreService.GetGenreName(genreId);
        var resultGenreId = _genreService.GetGenreId(genreName!);

        // Assert
        genreName.Should().NotBeNull();
        resultGenreId.Should().Be(genreId);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("action ")] // Trailing space
    [InlineData(" action")] // Leading space
    [InlineData(" action ")] // Both spaces
    public void GetGenreId_WithWhitespace_ReturnsNull(string genreName)
    {
        // Act
        var result = _genreService.GetGenreId(genreName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenreService_Constructor_InitializesCorrectly()
    {
        // Act & Assert - Service should be created without errors
        var service = new GenreService();
        service.Should().NotBeNull();
        
        // Verify it has the expected number of genres
        var allGenres = service.GetAllGenres();
        allGenres.Should().HaveCount(20);
    }

    #endregion
}
