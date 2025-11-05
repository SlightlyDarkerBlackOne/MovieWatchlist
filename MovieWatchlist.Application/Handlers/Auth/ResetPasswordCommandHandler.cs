using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<PasswordResetResponse>>
{
    private readonly IAuthenticationService _authService;

    public ResetPasswordCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<PasswordResetResponse>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request);
        
        return Result<PasswordResetResponse>.Success(result);
    }
}


