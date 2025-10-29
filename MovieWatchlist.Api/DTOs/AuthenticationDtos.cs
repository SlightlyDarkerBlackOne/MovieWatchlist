using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Api.DTOs;

public record RegisterDto(
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    string Username,
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    string Password
);

public record LoginDto(
    [Required(ErrorMessage = "Username or Email is required")]
    string UsernameOrEmail,
    
    [Required(ErrorMessage = "Password is required")]
    string Password
);

public record RefreshTokenDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);

public record ForgotPasswordDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email
);

public record ResetPasswordDto(
    [Required(ErrorMessage = "Reset token is required")]
    string Token,
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    string NewPassword,
    
    [Required(ErrorMessage = "Confirm password is required")]
    string ConfirmPassword
);

public record CreateRefreshTokenDto(
    [Required(ErrorMessage = "User ID is required")]
    int UserId
);

