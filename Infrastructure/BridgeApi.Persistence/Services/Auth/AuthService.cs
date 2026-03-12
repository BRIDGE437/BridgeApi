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
    private readonly GoogleSettings _googleSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<AppUser> userManager,
        ITokenProvider tokenProvider,
        IOptions<GoogleSettings> googleSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _googleSettings = googleSettings.Value;
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
}
