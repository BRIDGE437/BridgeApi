using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.File.UploadFile;

public record UploadFileCommandResponse(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    string Url,
    string ContentType,
    long Size,
    FileCategory Category,
    DateTime CreatedAt);
