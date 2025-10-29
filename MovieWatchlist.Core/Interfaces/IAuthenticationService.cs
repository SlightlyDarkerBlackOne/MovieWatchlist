using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IAuthenticationService
{
    Task<Result<User>> RegisterUserAsync(RegisterCommand command);
    AuthenticationResult GenerateAuthenticationResult(User user);
    Task<Result<RefreshTokenResult>> CreateRefreshTokenAsync(int userId);
    Task<Result<AuthenticationResult>> LoginAsync(LoginCommand command);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string token);
    Task<PasswordResetResponse> ForgotPasswordAsync(ForgotPasswordCommand command);
    Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordCommand command);
}
