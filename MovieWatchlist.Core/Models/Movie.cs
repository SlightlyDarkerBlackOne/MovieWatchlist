namespace MovieWatchlist.Core.Models;

public class Movie
{
    public int Id { get; set; }
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
} 