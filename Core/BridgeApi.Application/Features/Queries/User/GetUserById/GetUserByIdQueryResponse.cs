using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Queries.User.GetUserById;

public record GetUserByIdQueryResponse(string Id, string Username, string Email, UserRole Role, DateTime CreatedAt);
