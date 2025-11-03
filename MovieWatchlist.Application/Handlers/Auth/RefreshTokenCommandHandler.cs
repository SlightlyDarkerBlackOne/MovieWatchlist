using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authService;

    public RefreshTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var authResult = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Result<AuthenticationResult>.Success(authResult);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<AuthenticationResult>.Failure(ex.Message);
        }
    }
}
