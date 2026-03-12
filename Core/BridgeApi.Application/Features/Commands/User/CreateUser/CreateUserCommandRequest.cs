using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.User.CreateUser;

public record CreateUserCommandRequest(
    string Username,
    string Email,
    string Password,
    UserRole Role) : IRequest<CreateUserCommandResponse>;
