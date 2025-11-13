using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Infrastructure;

namespace MovieWatchlist.Tests.TestDataBuilders;

/// <summary>
/// Builder for creating PasswordResetToken test data using domain factory methods
/// </summary>
public class PasswordResetTokenBuilder
{
    private int _id = TestConstants.Users.DefaultUserId;
    private int _userId = TestConstants.Users.DefaultUserId;
    private string _token = Guid.NewGuid().ToString();
    private int _expirationHours = 1;
    private bool _isUsed = false;
    private DateTime? _createdAt = null;

    public PasswordResetTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PasswordResetTokenBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public PasswordResetTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public PasswordResetTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        var hoursUntilExpiration = (int)(expiresAt - DateTime.UtcNow).TotalHours;
        _expirationHours = Math.Max(1, hoursUntilExpiration);
        return this;
    }

    public PasswordResetTokenBuilder WithIsUsed(bool isUsed)
    {
        _isUsed = isUsed;
        return this;
    }

    public PasswordResetTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public PasswordResetToken Build()
    {
        var token = PasswordResetToken.Create(_userId, _token, _expirationHours);

        typeof(PasswordResetToken).GetProperty("Id")!.SetValue(token, _id);

        if (_createdAt.HasValue)
        {
            typeof(PasswordResetToken).GetProperty("CreatedAt")!.SetValue(token, _createdAt.Value);
        }

        if (_isUsed)
        {
            token.MarkAsUsed();
        }

        return token;
    }
}

