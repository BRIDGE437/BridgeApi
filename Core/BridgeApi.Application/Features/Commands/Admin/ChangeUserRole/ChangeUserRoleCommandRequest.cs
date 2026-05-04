using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;

public record ChangeUserRoleCommandRequest(string UserId, UserRole NewRole) : IRequest<ChangeUserRoleCommandResponse>;
