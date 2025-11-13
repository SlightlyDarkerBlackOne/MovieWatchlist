using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<PasswordResetResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IAuthenticationService authService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<PasswordResetResponse>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return Result<PasswordResetResponse>.Failure("New password and confirm password do not match");
        }

        var tokenPreview = string.IsNullOrEmpty(request.Token) ? "" : request.Token.Substring(0, Math.Min(8, request.Token.Length)) + "...";
        _logger.LogInformation("Password reset attempt with token: {Token}", tokenPreview);
        
        var result = await _authService.ResetPasswordAsync(request);
        
        return Result<PasswordResetResponse>.Success(result);
    }
}

