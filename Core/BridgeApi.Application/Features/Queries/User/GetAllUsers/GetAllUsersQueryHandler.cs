using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.AppUser;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Application.Features.Queries.User.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQueryRequest, GetAllUsersQueryResponse>
{
    private readonly IAppUserReadRepository _appUserReadRepository;
    private readonly UserManager<AppUserEntity> _userManager;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IAppUserReadRepository appUserReadRepository,
        UserManager<AppUserEntity> userManager,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _appUserReadRepository = appUserReadRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GetAllUsersQueryResponse> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _appUserReadRepository.GetAll(tracking: false);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.Pagination.Size);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Pagination.Page - 1) * request.Pagination.Size)
            .Take(request.Pagination.Size)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() is string roleName
                ? Enum.Parse<UserRole>(roleName)
                : UserRole.Founder;

            userDtos.Add(new UserDto(
                user.Id,
                user.UserName!,
                user.Email!,
                role,
                user.CreatedAt));
        }

        _logger.LogInformation("Retrieved {UserCount} users (page {Page}/{TotalPages})",
            userDtos.Count, request.Pagination.Page, totalPages);

        var paginatedResult = new PaginatedResponse<UserDto>(
            Items: userDtos,
            Page: request.Pagination.Page,
            Size: request.Pagination.Size,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPrevious: request.Pagination.Page > 1,
            HasNext: request.Pagination.Page < totalPages);

        return new GetAllUsersQueryResponse(paginatedResult);
    }
}
