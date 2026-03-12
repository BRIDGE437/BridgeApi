using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Dtos;

public record RegisterUserResult(string Id, string Username, string Email, UserRole Role, TokenDto Token);
