using Microsoft.AspNetCore.Http;

namespace BridgeApi.Application.Abstractions;

public interface ICsvImportService
{
    Task<(int success, int failed, string message)> ImportStartupsAsync(IFormFile file);
    Task<string> ClearImportedDataAsync();
    Task<(int userCount, int profileCount)> GetImportStatsAsync();
}
