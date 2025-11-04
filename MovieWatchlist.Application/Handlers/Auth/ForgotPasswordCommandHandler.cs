using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<PasswordResetResponse>>
{
    private readonly IAuthenticationService _authService;

    public ForgotPasswordCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<PasswordResetResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        
        return Result<PasswordResetResponse>.Success(result);
    }
}


