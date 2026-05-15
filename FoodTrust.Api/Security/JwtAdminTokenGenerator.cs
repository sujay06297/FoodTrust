using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodTrust.Api.Options;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FoodTrust.Api.Security;

public sealed class JwtAdminTokenGenerator(IOptions<AdminJwtOptions> options) : IAdminTokenGenerator
{
    /// <summary>
    /// 為指定管理員產生後台 JWT 存取權杖。
    /// </summary>
    public AdminAccessToken Generate(AdminUser user)
    {
        var jwtOptions = options.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Max(15, jwtOptions.ExpirationMinutes));
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AdminAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
