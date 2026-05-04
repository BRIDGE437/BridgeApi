using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;

public record ChangeUserRoleCommandBody(UserRole NewRole);
