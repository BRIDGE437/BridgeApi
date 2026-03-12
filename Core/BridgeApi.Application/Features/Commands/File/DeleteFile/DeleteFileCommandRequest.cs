using MediatR;

namespace BridgeApi.Application.Features.Commands.File.DeleteFile;

public record DeleteFileCommandRequest(
    Guid Id,
    string RequestingUserId,
    bool IsAdmin) : IRequest<DeleteFileCommandResponse?>;
