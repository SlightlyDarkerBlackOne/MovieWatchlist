using System.Text.RegularExpressions;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Core.ValueObjects;

public sealed class Username : IEquatable<Username>
{
    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Result<Username> Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<Username>.Failure("Username cannot be empty");
            
        if (username.Length < ValidationConstants.Username.MinLength || username.Length > ValidationConstants.Username.MaxLength)
            return Result<Username>.Failure(ValidationConstants.Username.InvalidLengthMessage);
            
        if (!IsValidFormat(username))
            return Result<Username>.Failure(ValidationConstants.Username.InvalidFormatMessage);
            
        return Result<Username>.Success(new Username(username));
    }

    private static bool IsValidFormat(string username)
    {
        return Regex.IsMatch(username, ValidationConstants.Username.Pattern);
    }

    public bool Equals(Username? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Username username && Equals(username);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    
    public static implicit operator string(Username username) => username.Value;
}

