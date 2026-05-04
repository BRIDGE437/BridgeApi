using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.User.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommandRequest, UpdateUserCommandResponse?>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        UserManager<AppUser> userManager,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UpdateUserCommandResponse?> Handle(UpdateUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
            return null;

        if (user.Id != request.RequestingUserId && !request.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to update this user.");

        if (request.Username != null)
            user.UserName = request.Username;
        if (request.Email != null)
            user.Email = request.Email;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User update failed for {UserId}: {Errors}", request.Id, errors);
            throw new InvalidOperationException($"User update failed: {errors}");
        }

        if (request.Role.HasValue)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role.Value.ToString());
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() is string roleName
            ? Enum.Parse<UserRole>(roleName)
            : UserRole.Founder;

        _logger.LogInformation("User {UserId} updated", user.Id);

        return new UpdateUserCommandResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            role,
            user.CreatedAt);
    }
}
