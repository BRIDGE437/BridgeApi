using MediatR;

namespace BridgeApi.Application.Features.Queries.File.GetFileById;

public record GetFileByIdQueryRequest(Guid Id) : IRequest<GetFileByIdQueryResponse?>;
