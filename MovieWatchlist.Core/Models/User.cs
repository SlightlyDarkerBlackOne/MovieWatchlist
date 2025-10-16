using System;
using System.Collections.Generic;

namespace MovieWatchlist.Core.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
} 