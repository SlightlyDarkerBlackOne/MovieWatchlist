using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    [Required(ErrorMessage = "Reset token is required")]
    string Token,
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    string NewPassword,
    
    [Required(ErrorMessage = "Confirm password is required")]
    string ConfirmPassword
) : IRequest<Result<PasswordResetResponse>>;

