using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Dtos;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Any authenticated user, regardless of role
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// Returns the profile of the currently logged-in user.
    /// code="200">Returns the user's own profile.
    /// code="401">The caller is not logged in.
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        int userId = this.GetCurrentUserId();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null || user.Role == null)
        {
            return NotFound();
        }

        return Ok(new UserProfileDto
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.RoleName,
            CreatedDate = user.CreatedDate
        });
    }

    /// Updates the currently logged-in user's own profile details.
    ///  code="200">Returns the updated profile.
    /// code="400">Validation failed.
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int userId = this.GetCurrentUserId();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null || user.Role == null)
        {
            return NotFound();
        }

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        await _db.SaveChangesAsync();

        return Ok(new UserProfileDto
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.RoleName,
            CreatedDate = user.CreatedDate
        });
    }
}
