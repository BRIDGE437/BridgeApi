using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using StoredFileEntity = BridgeApi.Domain.Entities.StoredFile;

namespace BridgeApi.Application.Features.Commands.File.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommandRequest, UploadFileCommandResponse>
{
    private readonly IStoredFileWriteRepository _storedFileWriteRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        IStoredFileWriteRepository storedFileWriteRepository,
        IStorageService storageService,
        ILogger<UploadFileCommandHandler> logger)
    {
        _storedFileWriteRepository = storedFileWriteRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<UploadFileCommandResponse> Handle(UploadFileCommandRequest request, CancellationToken cancellationToken)
    {
        var container = request.Category switch
        {
            FileCategory.ProfileImage => "profile-images",
            FileCategory.PostImage => "post-images",
            FileCategory.Document => "documents",
            _ => "others"
        };

        var extension = System.IO.Path.GetExtension(request.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var uploadResult = await _storageService.UploadAsync(
            request.FileStream, storedFileName, container, cancellationToken);

        var storedFile = new StoredFileEntity
        {
            OriginalFileName = request.FileName,
            StoredFileName = uploadResult.StoredFileName,
            Path = uploadResult.Path,
            Url = uploadResult.Url,
            Storage = _storageService.StorageName,
            ContentType = request.ContentType,
            Size = request.Size,
            Category = request.Category,
            UploadedByUserId = request.UploadedByUserId
        };

        await _storedFileWriteRepository.AddAsync(storedFile);
        await _storedFileWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} uploaded file {FileName} (Category: {Category}, Size: {Size})",
            request.UploadedByUserId, request.FileName, request.Category, request.Size);

        return new UploadFileCommandResponse(
            storedFile.Id,
            storedFile.OriginalFileName,
            storedFile.StoredFileName,
            storedFile.Url,
            storedFile.ContentType,
            storedFile.Size,
            storedFile.Category,
            storedFile.CreatedAt);
    }
}
