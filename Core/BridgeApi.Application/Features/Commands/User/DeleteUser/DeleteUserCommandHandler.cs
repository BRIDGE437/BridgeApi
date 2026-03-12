using BridgeApi.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.User.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest, DeleteUserCommandResponse?>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        UserManager<AppUser> userManager,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DeleteUserCommandResponse?> Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
            return null;

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User deletion failed for {UserId}: {Errors}", request.Id, errors);
            throw new InvalidOperationException($"User deletion failed: {errors}");
        }

        _logger.LogInformation("User {UserId} deleted", request.Id);

        return new DeleteUserCommandResponse();
    }
}
