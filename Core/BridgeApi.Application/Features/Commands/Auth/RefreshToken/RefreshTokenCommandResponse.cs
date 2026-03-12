using BridgeApi.Application.Dtos;

namespace BridgeApi.Application.Features.Commands.Auth.RefreshToken;

public record RefreshTokenCommandResponse(TokenDto Token);
