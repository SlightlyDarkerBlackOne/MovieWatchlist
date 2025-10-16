namespace MovieWatchlist.Core.Interfaces;

public interface IGenreService
{
    int? GetGenreId(string genreName);
    string? GetGenreName(int genreId);
    Dictionary<string, int> GetAllGenres();
}
