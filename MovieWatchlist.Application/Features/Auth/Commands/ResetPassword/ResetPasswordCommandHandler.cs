using MediatR;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;

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

