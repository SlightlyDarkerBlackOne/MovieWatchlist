using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MovieWatchlist.Api;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static MovieWatchlist.Tests.Infrastructure.TestConstants;

namespace MovieWatchlist.Tests.Controllers;

public class AuthControllerTests : EnhancedIntegrationTestBase
{
    public AuthControllerTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_SetsCookiesAndReturnsUser()
    {
        await InitializeDatabaseAsync();
        try
        {
            var registerRequest = new
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "NewPassword123!"
            };

            var response = await Client.PostAsJsonAsync(ApiEndpoints.AuthRegister, registerRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cookies = response.Headers.GetValues(HttpHeaders.SetCookie).ToList();
            cookies.Should().Contain(c => c.Contains(CookieNames.AccessToken));
            cookies.Should().Contain(c => c.Contains(CookieNames.RefreshToken));
            cookies.Should().OnlyContain(c => 
                c.Contains(CookieAttributes.HttpOnly, StringComparison.OrdinalIgnoreCase) && 
                c.Contains(CookieAttributes.SameSiteLax, StringComparison.OrdinalIgnoreCase));

            var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            content.Should().NotBeNull();
            content!.User.Should().NotBeNull();
            content.User.Username.Should().Be("newuser");
            content.User.Email.Should().Be("newuser@example.com");
            content.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_SetsCookiesAndReturnsUser()
    {
        await InitializeDatabaseAsync();
        try
        {
            await RegisterTestUserAsync();

            var loginRequest = new
            {
                UsernameOrEmail = Users.DefaultUsername,
                Password = Users.DefaultPassword
            };

            var response = await Client.PostAsJsonAsync(ApiEndpoints.AuthLogin, loginRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cookies = response.Headers.GetValues(HttpHeaders.SetCookie).ToList();
            cookies.Should().Contain(c => c.Contains(CookieNames.AccessToken));
            cookies.Should().Contain(c => c.Contains(CookieNames.RefreshToken));
            cookies.Should().OnlyContain(c => 
                c.Contains(CookieAttributes.HttpOnly, StringComparison.OrdinalIgnoreCase) && 
                c.Contains(CookieAttributes.SameSiteLax, StringComparison.OrdinalIgnoreCase));

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            content.Should().NotBeNull();
            content!.User.Should().NotBeNull();
            content.User.Username.Should().Be(Users.DefaultUsername);
            content.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            await RegisterTestUserAsync();

            var loginRequest = new
            {
                UsernameOrEmail = Users.DefaultUsername,
                Password = "WrongPassword123!"
            };

            var response = await Client.PostAsJsonAsync(ApiEndpoints.AuthLogin, loginRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            
            if (response.Headers.Contains(HttpHeaders.SetCookie))
            {
                var cookies = response.Headers.GetValues(HttpHeaders.SetCookie).ToList();
                cookies.Should().BeEmpty();
            }
            else
            {
                // No cookies should be set on failed login (which is correct)
                response.Headers.Contains(HttpHeaders.SetCookie).Should().BeFalse();
            }
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }


    [Fact]
    public async Task Me_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var response = await Client.GetAsync(ApiEndpoints.AuthMe);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }


    [Fact]
    public async Task Register_WithValidData_SetsCookiesWithCorrectConfiguration()
    {
        await InitializeDatabaseAsync();
        try
        {
            var registerRequest = new
            {
                Username = "cookieuser",
                Email = "cookieuser@example.com",
                Password = "CookiePassword123!"
            };

            var response = await Client.PostAsJsonAsync(ApiEndpoints.AuthRegister, registerRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();

            var cookies = response.Headers.GetValues(HttpHeaders.SetCookie).ToList();
            var accessTokenCookie = cookies.FirstOrDefault(c => c.Contains(CookieNames.AccessToken));
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains(CookieNames.RefreshToken));

            accessTokenCookie.Should().NotBeNull();
            refreshTokenCookie.Should().NotBeNull();

            accessTokenCookie.Should().ContainEquivalentOf(CookieAttributes.HttpOnly);
            accessTokenCookie.Should().ContainEquivalentOf(CookieAttributes.SameSiteLax);
            accessTokenCookie.Should().ContainEquivalentOf(CookieAttributes.Expires);

            refreshTokenCookie.Should().ContainEquivalentOf(CookieAttributes.HttpOnly);
            refreshTokenCookie.Should().ContainEquivalentOf(CookieAttributes.SameSiteLax);
            refreshTokenCookie.Should().ContainEquivalentOf(CookieAttributes.Expires);

            var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            content.Should().NotBeNull();
            content!.User.Should().NotBeNull();
            content.User.Should().BeOfType<UserInfo>();
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsDtoNotDomainModel()
    {
        await InitializeDatabaseAsync();
        try
        {
            await RegisterTestUserAsync();

            var loginRequest = new
            {
                UsernameOrEmail = Users.DefaultUsername,
                Password = Users.DefaultPassword
            };

            var response = await Client.PostAsJsonAsync(ApiEndpoints.AuthLogin, loginRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            content.Should().NotBeNull();
            content!.User.Should().NotBeNull();
            content.User.Should().BeOfType<UserInfo>();
            content.User.Id.Should().BeGreaterThan(0);
            content.User.Username.Should().Be(Users.DefaultUsername);
            content.User.Email.Should().Be(Users.DefaultEmail);
            content.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }
}

