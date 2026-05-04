namespace BridgeApi.Application.Abstractions.Services;

public interface IUserRoleService
{
    Task<string?> GetPrimaryRoleAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
}
