using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.MarkMessageAsRead;

public record MarkMessageAsReadCommandRequest(Guid Id) : IRequest<MarkMessageAsReadCommandResponse?>;
