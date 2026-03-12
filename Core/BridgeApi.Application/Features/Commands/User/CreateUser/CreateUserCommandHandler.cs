using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.User.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommandRequest, CreateUserCommandResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        UserManager<AppUser> userManager,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<CreateUserCommandResponse> Handle(CreateUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User creation failed for {Username}: {Errors}", request.Username, errors);
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role.ToString());

        _logger.LogInformation("User {UserId} created with role {Role}", user.Id, request.Role);

        return new CreateUserCommandResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            request.Role,
            user.CreatedAt);
    }
}
