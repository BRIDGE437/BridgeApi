namespace BridgeApi.Infrastructure.Configuration;

public class S3StorageSettings
{
    public string BucketName { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}
