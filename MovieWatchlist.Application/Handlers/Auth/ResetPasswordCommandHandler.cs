using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

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


