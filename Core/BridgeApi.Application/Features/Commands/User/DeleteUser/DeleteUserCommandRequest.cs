using MediatR;

namespace BridgeApi.Application.Features.Commands.User.DeleteUser;

public record DeleteUserCommandRequest(string Id) : IRequest<DeleteUserCommandResponse>;
