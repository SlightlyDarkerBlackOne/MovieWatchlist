using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Tests.Shared.TestDataBuilders;

/// <summary>
/// Builder for creating RefreshToken test data using domain factory methods
/// </summary>
public class RefreshTokenBuilder
{
    private int _id = TestConstants.Users.DefaultUserId;
    private int _userId = TestConstants.Users.DefaultUserId;
    private string _token = Guid.NewGuid().ToString();
    private int _expirationDays = TestConstants.Jwt.TestRefreshTokenExpirationDays;
    private bool _isRevoked = false;
    private DateTime? _createdAt = null;
    private DateTime? _expiresAt = null;

    public RefreshTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public RefreshTokenBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        var daysUntilExpiration = (expiresAt - DateTime.UtcNow).Days;
        _expirationDays = Math.Max(1, daysUntilExpiration);
        return this;
    }

    public RefreshTokenBuilder WithIsRevoked(bool isRevoked)
    {
        _isRevoked = isRevoked;
        return this;
    }

    public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public RefreshToken Build()
    {
        RefreshToken token;
        
        if (_expiresAt.HasValue || _createdAt.HasValue)
        {
            token = (RefreshToken)Activator.CreateInstance(typeof(RefreshToken), true)!;
            
            typeof(RefreshToken).GetProperty("UserId")!.SetValue(token, _userId);
            typeof(RefreshToken).GetProperty("Token")!.SetValue(token, _token);
            typeof(RefreshToken).GetProperty("IsRevoked")!.SetValue(token, false);
            
            var createdAtValue = _createdAt ?? DateTime.UtcNow;
            typeof(RefreshToken).GetProperty("CreatedAt")!.SetValue(token, createdAtValue);
            
            var expiresAtValue = _expiresAt ?? DateTime.UtcNow.AddDays(_expirationDays);
            typeof(RefreshToken).GetProperty("ExpiresAt")!.SetValue(token, expiresAtValue);
        }
        else
        {
            token = RefreshToken.Create(_userId, _token, _expirationDays);
        }

        if (_isRevoked)
        {
            token.Revoke();
        }

        return token;
    }
}

