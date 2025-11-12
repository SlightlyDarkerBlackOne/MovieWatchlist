using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordDto(
    [Required(ErrorMessage = "Reset token is required")]
    string Token,
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    string NewPassword,
    
    [Required(ErrorMessage = "Confirm password is required")]
    string ConfirmPassword
);

