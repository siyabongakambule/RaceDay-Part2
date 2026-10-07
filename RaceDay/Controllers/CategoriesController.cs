using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;
using RaceDay.Data;
using RaceDay.Dtos;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// Lists all categories for a specific event.
    ///  code="200">Returns the array of categories.
    [HttpGet("api/events/{eventId:int}/categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var categories = await _db.EventCategories
            .Where(c => c.EventID == eventId)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(categories);
    }

    /// Adds a new category (age or distance band) to an event.
    /// code="201">Returns the newly created category.
    ///  code="403">The caller does not own the parent event.
    /// code="404">The event does not exist.
    [HttpPost("api/events/{eventId:int}/categories")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null)
        {
            return NotFound(new { message = "Event does not exist." });
        }

        if (ev.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        var category = new EventCategory
        {
            EventID = eventId,
            CategoryName = dto.CategoryName,
            DistanceKm = dto.DistanceKm,
            EntryFee = dto.EntryFee,
            MaxParticipants = dto.MaxParticipants
        };

        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForEvent), new { eventId }, ToDto(category));
    }

    /// Updates a category's details.
    ///  code="200">Returns the updated category.
    ///  code="403">The caller does not own the parent event.
    ///  code="404">The category does not exist.
    [HttpPut("api/categories/{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _db.EventCategories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null || category.Event == null)
        {
            return NotFound(new { message = "Category does not exist." });
        }

        if (category.Event.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        category.CategoryName = dto.CategoryName;
        category.DistanceKm = dto.DistanceKm;
        category.EntryFee = dto.EntryFee;
        category.MaxParticipants = dto.MaxParticipants;

        await _db.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    /// Deletes a category, only if it has no enrolments
    /// <response code="200">Category deleted successfully.
    ///  code="403">The caller does not own the parent event.
    ///  code="404">The category does not exist.
    ///  code="409">The category has active enrolments and cannot be deleted.
    [HttpDelete("api/categories/{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.EventCategories
            .Include(c => c.Event)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null || category.Event == null)
        {
            return NotFound(new { message = "Category does not exist." });
        }

        if (category.Event.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        if (category.Enrollments.Any())
        {
            return Conflict(new { message = "This category has active enrolments and cannot be deleted." });
        }

        _db.EventCategories.Remove(category);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Category deleted." });
    }

    private static CategoryDto ToDto(EventCategory c) => new()
    {
        CategoryID = c.CategoryID,
        EventID = c.EventID,
        CategoryName = c.CategoryName,
        DistanceKm = c.DistanceKm,
        EntryFee = c.EntryFee,
        MaxParticipants = c.MaxParticipants
    };
}
