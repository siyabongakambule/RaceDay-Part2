using RaceDay.Models;

namespace RaceDay.Services;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) CreateToken(User user, string roleName);
}
