using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.Register;

public record RegisterCommandRequest(
    string Username,
    string Email,
    string Password,
    UserRole Role) : IRequest<RegisterCommandResponse>;
