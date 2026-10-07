using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Models;

namespace RaceDay.Services;

// Issues a JWT that the client sends on every subsequent request
// (Authorization: Bearer <token>) so the API can identify the user
// and their role without needing server-side session storage.
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, DateTime expiresAtUtc) CreateToken(User user, string roleName)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        string key = jwtSettings["Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        string issuer = jwtSettings["Issuer"] ?? "Api";
        string audience = jwtSettings["Audience"] ?? "Client";
        int expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var m) ? m : 120;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, roleName)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        DateTime expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}
