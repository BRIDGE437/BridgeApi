using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.GoogleLogin;

public record GoogleLoginCommandRequest(
    string IdToken,
    UserRole Role) : IRequest<GoogleLoginCommandResponse>;
