using BridgeApi.Application.Dtos;
using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.Auth.Register;

public record RegisterCommandResponse(
    string Id,
    string Username,
    string Email,
    UserRole Role,
    TokenDto Token);
