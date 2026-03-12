using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.File.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommandRequest, DeleteFileCommandResponse?>
{
    private readonly IStoredFileReadRepository _storedFileReadRepository;
    private readonly IStoredFileWriteRepository _storedFileWriteRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(
        IStoredFileReadRepository storedFileReadRepository,
        IStoredFileWriteRepository storedFileWriteRepository,
        IStorageService storageService,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _storedFileReadRepository = storedFileReadRepository;
        _storedFileWriteRepository = storedFileWriteRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<DeleteFileCommandResponse?> Handle(DeleteFileCommandRequest request, CancellationToken cancellationToken)
    {
        var storedFile = await _storedFileReadRepository.GetByIdAsync(request.Id);
        if (storedFile == null)
            return null;

        if (storedFile.UploadedByUserId != request.RequestingUserId && !request.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this file.");

        await _storageService.DeleteAsync(storedFile.Path, cancellationToken);
        await _storedFileWriteRepository.RemoveAsync(storedFile);
        await _storedFileWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} deleted file {FileId}",
            request.RequestingUserId, request.Id);

        return new DeleteFileCommandResponse();
    }
}
