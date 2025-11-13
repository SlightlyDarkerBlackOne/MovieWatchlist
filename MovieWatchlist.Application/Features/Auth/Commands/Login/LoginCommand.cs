using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    [Required(ErrorMessage = "Username or Email is required")]
    string UsernameOrEmail,
    
    [Required(ErrorMessage = "Password is required")]
    string Password
) : IRequest<Result<LoginResponse>>;

