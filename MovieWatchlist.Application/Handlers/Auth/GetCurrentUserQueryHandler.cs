using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserInfo>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result<UserInfo>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<UserInfo>.Failure("User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Result<UserInfo>.Failure("User not found");
        }

        var userInfo = new UserInfo(
            Id: user.Id,
            Username: user.Username.Value,
            Email: user.Email.Value,
            CreatedAt: user.CreatedAt
        );

        return Result<UserInfo>.Success(userInfo);
    }
}

