using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Application.Features.Auth.Commands.Login;

public record LoginDto(
    [Required(ErrorMessage = "Username or Email is required")]
    string UsernameOrEmail,
    
    [Required(ErrorMessage = "Password is required")]
    string Password
);

