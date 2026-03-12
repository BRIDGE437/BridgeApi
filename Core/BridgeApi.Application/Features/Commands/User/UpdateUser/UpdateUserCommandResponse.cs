using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.User.UpdateUser;

public record UpdateUserCommandResponse(
    string Id,
    string Username,
    string Email,
    UserRole Role,
    DateTime CreatedAt);
