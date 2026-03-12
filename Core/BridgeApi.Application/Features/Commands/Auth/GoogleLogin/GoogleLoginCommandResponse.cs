using BridgeApi.Application.Dtos;

namespace BridgeApi.Application.Features.Commands.Auth.GoogleLogin;

public record GoogleLoginCommandResponse(
    string Id,
    string Username,
    string Email,
    TokenDto Token);
