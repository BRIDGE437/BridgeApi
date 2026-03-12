using BridgeApi.Application.Dtos;

namespace BridgeApi.Application.Features.Commands.Auth.Login;

public record LoginCommandResponse(
    string Id,
    string Username,
    string Email,
    TokenDto Token);
