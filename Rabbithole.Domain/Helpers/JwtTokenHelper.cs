using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.Interfaces.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rabbithole.Domain.Helpers
{
    public class JwtTokenHelper(IConfiguration configuration) : IJwtTokenHelper
    {
        public const string Issuer = "rabbithole";
        public const string Audience = "rabbithole";
        public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

        public string CreateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NavidromeUsername),
                new Claim(ClaimTypes.Role, user.IsAdmin ? UserRoles.Admin : UserRoles.User)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]!));

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(AccessTokenLifetime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
