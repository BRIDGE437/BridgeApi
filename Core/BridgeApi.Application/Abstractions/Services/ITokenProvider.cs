using BridgeApi.Application.Dtos;
using BridgeApi.Domain.Entities;

namespace BridgeApi.Application.Abstractions.Services;

public interface ITokenProvider
{
    Task<TokenDto> CreateTokenAsync(AppUser user);
    string CreateRefreshToken();
    string GetUserIdFromExpiredToken(string accessToken);
}
