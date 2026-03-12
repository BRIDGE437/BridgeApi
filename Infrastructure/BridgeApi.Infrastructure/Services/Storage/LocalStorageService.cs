using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos;
using BridgeApi.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BridgeApi.Infrastructure.Services.Storage;

public class LocalStorageService : IStorageService
{
    private readonly LocalStorageSettings _settings;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IOptions<LocalStorageSettings> settings, ILogger<LocalStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string StorageName => "Local";

    public async Task<StorageUploadResult> UploadAsync(Stream stream, string fileName, string container, CancellationToken cancellationToken = default)
    {
        var directoryPath = Path.Combine("wwwroot", _settings.BasePath, container);
        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var relativePath = $"{container}/{fileName}";
        var url = GetPublicUrl(relativePath);

        _logger.LogInformation("File uploaded to local storage: {Path}", relativePath);

        return new StorageUploadResult(fileName, relativePath, url);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine("wwwroot", _settings.BasePath, path);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
            _logger.LogInformation("File deleted from local storage: {Path}", path);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string path)
    {
        return $"{_settings.BaseUrl.TrimEnd('/')}/{path}";
    }
}
