using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.RefreshToken;

public record RefreshTokenCommandRequest(
    string AccessToken,
    string RefreshToken) : IRequest<RefreshTokenCommandResponse>;
