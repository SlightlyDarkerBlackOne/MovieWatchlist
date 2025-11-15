using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Tests.Shared.TestDataBuilders;

/// <summary>
/// Builder for creating User test data
/// </summary>
public class UserBuilder
{
    private int _id = TestConstants.Users.DefaultUserId;
    private string _username = TestConstants.Users.DefaultUsername;
    private string _email = TestConstants.Users.DefaultEmail;
    private string _passwordHash = TestConstants.Users.DefaultPasswordHash;
    private DateTime? _lastLoginAt = null;

    public UserBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        return this;
    }

    public UserBuilder WithLastLoginAt(DateTime? lastLoginAt)
    {
        _lastLoginAt = lastLoginAt;
        return this;
    }

    public User Build()
    {
        var username = Username.Create(_username).Value!;
        var email = Email.Create(_email).Value!;
        
        var user = User.Create(username, email, _passwordHash);
        
        typeof(User).GetProperty("Id")!.SetValue(user, _id);
        
        if (_lastLoginAt.HasValue)
        {
            typeof(User).GetProperty("LastLoginAt")!.SetValue(user, _lastLoginAt.Value);
        }
        
        return user;
    }
}

