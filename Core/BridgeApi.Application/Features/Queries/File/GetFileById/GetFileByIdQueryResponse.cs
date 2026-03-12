using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Queries.File.GetFileById;

public record GetFileByIdQueryResponse(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    string Url,
    string ContentType,
    long Size,
    FileCategory Category,
    string UploadedByUserId,
    DateTime CreatedAt);
