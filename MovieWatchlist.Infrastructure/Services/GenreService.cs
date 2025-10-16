using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Services;

public class GenreService : IGenreService
{
    private readonly Dictionary<string, int> _genreMap = new(StringComparer.OrdinalIgnoreCase)
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

    private readonly Dictionary<int, string> _reverseGenreMap;

    public GenreService()
    {
        _reverseGenreMap = _genreMap
            .GroupBy(kvp => kvp.Value)
            .ToDictionary(g => g.Key, g => g.First().Key);
    }

    public int? GetGenreId(string genreName)
    {
        if (string.IsNullOrWhiteSpace(genreName)) return null;
        return _genreMap.TryGetValue(genreName, out var genreId) ? genreId : null;
    }

    public string? GetGenreName(int genreId)
    {
        return _reverseGenreMap.TryGetValue(genreId, out var genreName) ? genreName : null;
    }

    public Dictionary<string, int> GetAllGenres()
    {
        return new Dictionary<string, int>(_genreMap);
    }
}
