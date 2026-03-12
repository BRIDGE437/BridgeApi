using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.File.UploadFile;

public record UploadFileCommandRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    long Size,
    FileCategory Category,
    string UploadedByUserId) : IRequest<UploadFileCommandResponse>;
