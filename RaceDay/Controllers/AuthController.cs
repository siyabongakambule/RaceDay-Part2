using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;
using RaceDay.Data;
using RaceDay.Dtos;
using RaceDay.Models;
using RaceDay.Services;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(ApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    /// Registers a new user as either an Organiser or a Participant.
    /// Users cannot self-register as Admin. Passwords are hashed before being stored.
    ///  code="201">User created successfully.
    ///  code="400">Validation failed, or the role supplied is not allowed.
    ///  code="409">A user with this email already exists.
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Only Organiser or Participant may be chosen at registration.
        if (dto.Role != RoleNames.Organiser && dto.Role != RoleNames.Participant)
        {
            return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });
        }

        bool emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return Conflict(new { message = "A user with this email is already registered." });
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
        if (role == null)
        {
            return BadRequest(new { message = "The selected role does not exist." });
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = PasswordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            RoleID = role.RoleID,
            CreatedDate = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var profile = new UserProfileDto
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = role.RoleName,
            CreatedDate = user.CreatedDate
        };

        return CreatedAtAction(nameof(Register), new { id = user.UserID }, profile);
    }

    /// Authenticates a user and returns a JWT access token.
    /// The token must be sent as "Authorization: Bearer &lt;token&gt;" on subsequent requests
    ///  code="200">Login successful - returns the token and user profile.
    /// code="401">The email or password is incorrect.
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || user.Role == null || !PasswordHasher.Verify(dto.Password, user.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var (token, expires) = _tokenService.CreateToken(user, user.Role.RoleName);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expires,
            User = new UserProfileDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.RoleName,
                CreatedDate = user.CreatedDate
            }
        };

        return Ok(response);
    }
}
