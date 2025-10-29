using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Core.ValueObjects;

public sealed class Rating : IEquatable<Rating>
{
    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Result<Rating> Create(int value)
    {
        if (value < ValidationConstants.Rating.MinValue || value > ValidationConstants.Rating.MaxValue)
            return Result<Rating>.Failure(ValidationConstants.Rating.InvalidRangeMessage);
            
        return Result<Rating>.Success(new Rating(value));
    }

    public bool Equals(Rating? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Rating rating && Equals(rating);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    
    public static implicit operator int(Rating rating) => rating.Value;
}

