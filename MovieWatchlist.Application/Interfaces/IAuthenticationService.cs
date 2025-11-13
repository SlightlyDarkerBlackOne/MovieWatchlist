using MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<User>> RegisterUserAsync(RegisterCommand command);
    AuthenticationResult GenerateAuthenticationResult(User user);
    Task<AuthenticationResult> GenerateAuthenticationResultWithRefreshTokenAsync(User user);
    Task<Result<RefreshTokenResult>> CreateRefreshTokenAsync(int userId);
    Task<Result<AuthenticationResult>> LoginAsync(LoginCommand command);
    Task<bool> ValidateTokenAsync(string token);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string token);
    Task<PasswordResetResponse> ForgotPasswordAsync(ForgotPasswordCommand command);
    Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordCommand command);
}

