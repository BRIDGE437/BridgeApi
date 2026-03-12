using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using MediatR;

namespace BridgeApi.Application.Features.Queries.File.GetFileById;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQueryRequest, GetFileByIdQueryResponse?>
{
    private readonly IStoredFileReadRepository _storedFileReadRepository;

    public GetFileByIdQueryHandler(IStoredFileReadRepository storedFileReadRepository)
    {
        _storedFileReadRepository = storedFileReadRepository;
    }

    public async Task<GetFileByIdQueryResponse?> Handle(GetFileByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var storedFile = await _storedFileReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (storedFile == null)
            return null;

        return new GetFileByIdQueryResponse(
            storedFile.Id,
            storedFile.OriginalFileName,
            storedFile.StoredFileName,
            storedFile.Url,
            storedFile.ContentType,
            storedFile.Size,
            storedFile.Category,
            storedFile.UploadedByUserId,
            storedFile.CreatedAt);
    }
}
