using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;

public record ChangeUserRoleCommandResponse(string UserId, UserRole NewRole, bool Success);
