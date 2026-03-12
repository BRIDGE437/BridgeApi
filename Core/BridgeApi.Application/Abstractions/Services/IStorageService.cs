using BridgeApi.Application.Dtos;

namespace BridgeApi.Application.Abstractions.Services;

public interface IStorageService
{
    Task<StorageUploadResult> UploadAsync(Stream stream, string fileName, string container, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    string GetPublicUrl(string path);
    string StorageName { get; }
}
