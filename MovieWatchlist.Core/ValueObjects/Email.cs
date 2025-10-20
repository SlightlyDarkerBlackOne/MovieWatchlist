using System.Text.RegularExpressions;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Core.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("Email cannot be empty");
            
        if (email.Length > ValidationConstants.Email.MaxLength)
            return Result<Email>.Failure(ValidationConstants.Email.InvalidLengthMessage);
            
        if (!IsValidFormat(email))
            return Result<Email>.Failure(ValidationConstants.Email.InvalidFormatMessage);
            
        return Result<Email>.Success(new Email(email.ToLowerInvariant()));
    }

    private static bool IsValidFormat(string email)
    {
        return Regex.IsMatch(email, ValidationConstants.Email.Pattern);
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    
    public static implicit operator string(Email email) => email.Value;
}

