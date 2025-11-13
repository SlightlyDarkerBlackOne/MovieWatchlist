using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<PasswordResetResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IAuthenticationService authService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<Result<PasswordResetResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset requested for email: {Email}", request.Email);
        
        var result = await _authService.ForgotPasswordAsync(request);
        
        return Result<PasswordResetResponse>.Success(result);
    }
}

