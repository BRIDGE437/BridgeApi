using Amazon.S3;
using Amazon.S3.Model;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos;
using BridgeApi.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BridgeApi.Infrastructure.Services.Storage;

public class S3StorageService : IStorageService
{
    private readonly S3StorageSettings _settings;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IOptions<S3StorageSettings> settings,
        IAmazonS3 s3Client,
        ILogger<S3StorageService> logger)
    {
        _settings = settings.Value;
        _s3Client = s3Client;
        _logger = logger;
    }

    public string StorageName => "S3";

    public async Task<StorageUploadResult> UploadAsync(Stream stream, string fileName, string container, CancellationToken cancellationToken = default)
    {
        var key = $"{container}/{fileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = GetContentType(fileName)
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        var url = GetPublicUrl(key);

        _logger.LogInformation("File uploaded to S3: {Key}", key);

        return new StorageUploadResult(fileName, key, url);
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = path
        };

        await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);

        _logger.LogInformation("File deleted from S3: {Key}", path);
    }

    public string GetPublicUrl(string path)
    {
        return $"{_settings.BaseUrl.TrimEnd('/')}/{path}";
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
