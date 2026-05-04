using BridgeApi.Application.Dtos;
using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Abstractions.Services;

public interface IAuthService
{
    Task<RegisterUserResult> RegisterAsync(string username, string email, string password, UserRole role);
    Task<LoginUserResult> LoginAsync(string usernameOrEmail, string password);
    Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<LoginUserResult> GoogleLoginAsync(string googleIdToken, UserRole role);

    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);

    Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

    Task<PasswordResetResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole, CancellationToken cancellationToken);
}
