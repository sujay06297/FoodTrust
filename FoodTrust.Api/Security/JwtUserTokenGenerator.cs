using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodTrust.Api.Options;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FoodTrust.Api.Security;

public sealed class JwtUserTokenGenerator(IOptions<UserJwtOptions> options) : IUserTokenGenerator
{
    /// <summary>
    /// 為指定會員產生 JWT 存取權杖。
    /// </summary>
    public UserAccessToken Generate(User user)
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
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, UserRole.User)
        };
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new UserAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
