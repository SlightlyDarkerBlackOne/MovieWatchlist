using System.Security.Claims;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(id, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}


