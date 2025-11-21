using Mapster;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Commands.Login;

namespace MovieWatchlist.Api.Mapping;

public class AuthMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuthenticationResult, RegisterResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, LoginResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, RefreshTokenResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);
    }
}

