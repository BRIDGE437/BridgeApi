using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.User.UpdateUser;

public record UpdateUserCommandRequest(
    string Id,
    string? Username,
    string? Email,
    UserRole? Role,
    string RequestingUserId,
    bool IsAdmin) : IRequest<UpdateUserCommandResponse>;
