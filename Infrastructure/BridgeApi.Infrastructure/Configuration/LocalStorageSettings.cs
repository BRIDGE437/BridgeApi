namespace BridgeApi.Infrastructure.Configuration;

public class LocalStorageSettings
{
    public string BasePath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "https://localhost:7065/uploads";
}
