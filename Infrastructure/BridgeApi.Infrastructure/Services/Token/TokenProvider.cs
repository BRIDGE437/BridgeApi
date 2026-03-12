using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos;
using BridgeApi.Domain.Entities;
using BridgeApi.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BridgeApi.Infrastructure.Services.Token;

public class TokenProvider : ITokenProvider
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public TokenProvider(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenDto> CreateTokenAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiration,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = CreateRefreshToken();

        return new TokenDto(accessToken, refreshToken, expiration);
    }

    public string CreateRefreshToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string GetUserIdFromExpiredToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid access token.");

        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new SecurityTokenException("Token does not contain a user identifier.");
    }
}
