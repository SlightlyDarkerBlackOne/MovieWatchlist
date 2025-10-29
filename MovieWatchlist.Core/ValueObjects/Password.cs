using System.Text.RegularExpressions;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Core.ValueObjects;

public sealed class Password : IEquatable<Password>
{
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<Password>.Failure(ValidationConstants.Password.InvalidFormatMessage);
            
        if (password.Length < ValidationConstants.Password.MinLength || 
            password.Length > ValidationConstants.Password.MaxLength)
            return Result<Password>.Failure(ValidationConstants.Password.InvalidFormatMessage);
            
        if (!IsValidFormat(password))
            return Result<Password>.Failure(ValidationConstants.Password.InvalidFormatMessage);
            
        return Result<Password>.Success(new Password(password));
    }

    private static bool IsValidFormat(string password)
    {
        return Regex.IsMatch(password, ValidationConstants.Password.Pattern);
    }

    public bool Equals(Password? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Password password && Equals(password);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => "********";
}

