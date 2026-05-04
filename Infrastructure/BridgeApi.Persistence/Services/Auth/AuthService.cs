using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos;
using BridgeApi.Application.Exceptions;
using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BridgeApi.Persistence.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUserProfileWriteRepository _userProfileWriteRepository;
    private readonly IEmailService _emailService;
    private readonly GoogleSettings _googleSettings;
    private readonly PasswordResetSettings _resetSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<AppUser> userManager,
        ITokenProvider tokenProvider,
        IUserProfileWriteRepository userProfileWriteRepository,
        IEmailService emailService,
        IOptions<GoogleSettings> googleSettings,
        IOptions<PasswordResetSettings> resetSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _userProfileWriteRepository = userProfileWriteRepository;
        _emailService = emailService;
        _googleSettings = googleSettings.Value;
        _resetSettings = resetSettings.Value;
        _logger = logger;
    }

    public async Task<RegisterUserResult> RegisterAsync(string username, string email, string password, UserRole role)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = username,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AuthenticationException(errors);
        }

        var roleName = role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        var profile = new UserProfile
        {
            UserId = user.Id
        };
        await _userProfileWriteRepository.AddAsync(profile);
        await _userProfileWriteRepository.SaveAsync();

        var token = await _tokenProvider.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} registered with role {Role}", user.Id, roleName);

        return new RegisterUserResult(user.Id, user.UserName!, user.Email!, role, token);
    }

    public async Task<LoginUserResult> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail)
                   ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        if (user is null)
            throw new AuthenticationException("Invalid username/email or password.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            throw new AuthenticationException("Invalid username/email or password.");

        var token = await _tokenProvider.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} logged in", user.Id);

        return new LoginUserResult(user.Id, user.UserName!, user.Email!, token);
    }

    public async Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        string userId;
        try
        {
            userId = _tokenProvider.GetUserIdFromExpiredToken(accessToken);
        }
        catch
        {
            throw new AuthenticationException("Invalid access token.");
        }

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new AuthenticationException("User not found.");

        if (user.RefreshToken != refreshToken)
            throw new AuthenticationException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new AuthenticationException("Refresh token has expired.");

        var token = await _tokenProvider.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed for user {UserId}", userId);

        return token;
    }

    public async Task<LoginUserResult> GoogleLoginAsync(string googleIdToken, UserRole role)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
        }
        catch (InvalidJwtException)
        {
            throw new AuthenticationException("Invalid Google ID token.");
        }

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = payload.Email.Split('@')[0],
                Email = payload.Email,
                EmailConfirmed = true,
                AuthProvider = "Google",
                ProviderKey = payload.Subject
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new AuthenticationException(errors);
            }

            var roleName = role.ToString();
            await _userManager.AddToRoleAsync(user, roleName);

            var profile = new UserProfile
            {
                UserId = user.Id,
                Name = payload.Name
            };
            await _userProfileWriteRepository.AddAsync(profile);
            await _userProfileWriteRepository.SaveAsync();

            _logger.LogInformation("User {UserId} registered via Google with role {Role}", user.Id, roleName);
        }
        else
        {
            _logger.LogInformation("User {UserId} logged in via Google", user.Id);
        }

        var token = await _tokenProvider.CreateTokenAsync(user);

        user.RefreshToken = token.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginUserResult(user.Id, user.UserName!, user.Email!, token);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for unknown email (ignored).");
            return;
        }

        if (!string.IsNullOrEmpty(user.AuthProvider) &&
            !user.AuthProvider.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Password reset skipped for {UserId}: external provider {Provider}.", user.Id, user.AuthProvider);
            return;
        }

        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(rawToken);
        var encodedEmail = Uri.EscapeDataString(user.Email!);

        var baseUrl = _resetSettings.FrontendBaseUrl.TrimEnd('/');
        var path = _resetSettings.ResetPath.StartsWith('/') ? _resetSettings.ResetPath : "/" + _resetSettings.ResetPath;
        var resetUrl = $"{baseUrl}{path}?email={encodedEmail}&token={encodedToken}";

        await _emailService.SendPasswordResetAsync(
            user.Email!,
            user.UserName ?? user.Email!,
            resetUrl,
            _resetSettings.TokenLifetimeMinutes,
            cancellationToken);

        _logger.LogInformation("Password reset link issued for {UserId}", user.Id);
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogInformation("Password reset attempted for unknown email (ignored).");
            return new PasswordResetResult(false, new[] { "Invalid or expired token." });
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var codes = result.Errors.Select(e => e.Code).ToArray();
            _logger.LogInformation("Password reset failed for {UserId}: {Codes}", user.Id, string.Join(",", codes));

            var errors = result.Errors.Select(e => e.Description).ToArray();
            return new PasswordResetResult(false, errors);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password reset completed for {UserId}", user.Id);

        try
        {
            await _emailService.SendPasswordChangedAsync(
                user.Email!,
                user.UserName ?? user.Email!,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password-changed notification enqueue failed for {UserId}", user.Id);
        }

        return new PasswordResetResult(true, null);
    }

    public async Task<PasswordResetResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new PasswordResetResult(false, new[] { "User not found." });

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var codes = result.Errors.Select(e => e.Code).ToArray();
            _logger.LogInformation("Password change failed for {UserId}: {Codes}", userId, string.Join(",", codes));
            return new PasswordResetResult(false, result.Errors.Select(e => e.Description).ToArray());
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password changed for {UserId}", userId);

        try
        {
            await _emailService.SendPasswordChangedAsync(user.Email!, user.UserName ?? user.Email!, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password-changed notification enqueue failed for {UserId}", userId);
        }

        return new PasswordResetResult(true, null);
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var newRoleName = newRole.ToString();

        if (currentRoles.Contains(newRoleName) && currentRoles.Count == 1)
            return true;

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            throw new AuthenticationException(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

        var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
        if (!addResult.Succeeded)
            throw new AuthenticationException(string.Join(", ", addResult.Errors.Select(e => e.Description)));

        await _userManager.UpdateSecurityStampAsync(user);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} role changed from [{OldRoles}] to {NewRole}", userId, string.Join(",", currentRoles), newRoleName);
        return true;
    }
}
