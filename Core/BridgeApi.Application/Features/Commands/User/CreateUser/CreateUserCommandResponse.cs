using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.User.CreateUser;

public record CreateUserCommandResponse(
    string Id,
    string Username,
    string Email,
    UserRole Role,
    DateTime CreatedAt);
