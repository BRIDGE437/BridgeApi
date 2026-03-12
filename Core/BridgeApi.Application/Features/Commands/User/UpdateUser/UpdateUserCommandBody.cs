using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.User.UpdateUser;

/// <summary>
/// Request body for PUT /api/User/{id}. Id comes from route.
/// </summary>
public record UpdateUserCommandBody(string? Username, string? Email, UserRole? Role);
