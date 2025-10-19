using System.Security.Claims;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}


