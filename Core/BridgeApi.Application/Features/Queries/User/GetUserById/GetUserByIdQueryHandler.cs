using BridgeApi.Application.Abstractions.Repositories.AppUser;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Application.Features.Queries.User.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQueryRequest, GetUserByIdQueryResponse?>
{
    private readonly IAppUserReadRepository _appUserReadRepository;
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IAppUserReadRepository appUserReadRepository,
        UserManager<AppUserEntity> userManager,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _appUserReadRepository = appUserReadRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GetUserByIdQueryResponse?> Handle(GetUserByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var user = await _appUserReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() is string roleName
            ? Enum.Parse<UserRole>(roleName)
            : UserRole.Founder;

        _logger.LogInformation("Retrieved user {UserId}", user.Id);

        return new GetUserByIdQueryResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            role,
            user.CreatedAt);
    }
}
