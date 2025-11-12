using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordDto(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email
);

