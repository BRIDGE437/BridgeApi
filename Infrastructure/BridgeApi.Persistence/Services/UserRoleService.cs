using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BridgeApi.Persistence.Services;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<AppUser> _userManager;

    public UserRoleService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetPrimaryRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return roles
            .OrderBy(r => r == "Admin" ? 0 : 1)
            .FirstOrDefault();
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }
}
