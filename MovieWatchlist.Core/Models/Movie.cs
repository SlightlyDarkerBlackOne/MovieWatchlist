namespace MovieWatchlist.Core.Models;

public class Movie
{
    public int Id { get; private set; }
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string PosterPath { get; set; } = string.Empty;
    public string? BackdropPath { get; set; }
    public DateTime ReleaseDate { get; set; }
    public double VoteAverage { get; set; }
    public int VoteCount { get; set; }
    public double Popularity { get; set; }
    public string[] Genres { get; set; } = Array.Empty<string>();   
    public string? CreditsJson { get; set; }    
    public string? VideosJson { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public bool IsRecent => ReleaseDate >= DateTime.UtcNow.AddYears(-1);
    public bool IsPopular => VoteCount >= 1000 && VoteAverage >= 7.0;
    public bool IsClassic => ReleaseDate.Year < 2000;
    public bool IsHighlyRated => VoteAverage >= 8.0 && VoteCount >= 100;
    public string FormattedReleaseYear => ReleaseDate.ToString("yyyy");
    
    // Helper methods for display
    public string GetPosterUrl(string baseUrl = "https://image.tmdb.org/t/p/w500")
    {
        return string.IsNullOrEmpty(PosterPath) 
            ? string.Empty 
            : $"{baseUrl}{PosterPath}";
    }
    
    public string GetBackdropUrl(string baseUrl = "https://image.tmdb.org/t/p/w1280")
    {
        return string.IsNullOrEmpty(BackdropPath) 
            ? string.Empty 
            : $"{baseUrl}{BackdropPath}";
    }
    
    public bool HasGenre(string genre) => Genres.Contains(genre);
    
    public bool HasAnyGenre(params string[] genres) => genres.Any(genre => Genres.Contains(genre));
    
    public string GetGenresAsString(string separator = ", ") => string.Join(separator, Genres);
    
    public string GetTruncatedOverview(int maxLength = 150)
    {
        if (string.IsNullOrEmpty(Overview))
            return string.Empty;
        
        if (Overview.Length <= maxLength)
            return Overview;
        
        return Overview.Substring(0, maxLength).TrimEnd() + "...";
    }
}
