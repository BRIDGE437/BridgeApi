using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Queries.User.GetAllUsers;

public record GetAllUsersQueryResponse(PaginatedResponse<UserDto> Data);

public record UserDto(string Id, string Username, string Email, UserRole Role, DateTime CreatedAt);
