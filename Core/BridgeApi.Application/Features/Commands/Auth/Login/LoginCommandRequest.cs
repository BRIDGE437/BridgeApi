using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.Login;

public record LoginCommandRequest(
    string UsernameOrEmail,
    string Password) : IRequest<LoginCommandResponse>;
