using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;

using RaceDay.Data;
using RaceDay.Dtos;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EnrolmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// Enrols the logged-in participant into a category for an event.
    ///  code="201">Returns the new enrolment record with status "Pending".
    ///  code="403">The caller is not a Participant.
    ///  code="404">The category does not exist.
    /// code="409">The participant is already enrolled in this category.
    [HttpPost("api/categories/{categoryId:int}/enrol")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int categoryId)
    {
        var category = await _db.EventCategories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == categoryId);
        if (category == null)
        {
            return NotFound(new { message = "Category does not exist." });
        }

        int participantId = this.GetCurrentUserId();

        bool alreadyEnrolled = await _db.Enrollments
            .AnyAsync(en => en.ParticipantID == participantId && en.CategoryID == categoryId);

        if (alreadyEnrolled)
        {
            return Conflict(new { message = "You are already enrolled in this category." });
        }

        var enrolment = new Enrollment
        {
            ParticipantID = participantId,
            CategoryID = categoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Pending"
        };

        _db.Enrollments.Add(enrolment);
        await _db.SaveChangesAsync();

        var dto = await LoadDto(enrolment.EnrollmentID);
        return CreatedAtAction(nameof(GetMyEnrolments), null, dto);
    }

    /// Lists all events/categories the logged-in participant has enrolled in.
    ///  code="200">Returns the array of the caller's own enrolments.
    [HttpGet("api/users/me/enrolments")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEnrolments()
    {
        int participantId = this.GetCurrentUserId();

        var enrolments = await _db.Enrollments
            .Include(en => en.Participant)
            .Include(en => en.Category).ThenInclude(c => c!.Event)
            .Where(en => en.ParticipantID == participantId)
            .Select(en => ToDto(en))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// Lists all participants enrolled in an event, for the organiser to manage.
    ///  code="200">Returns the array of enrolments for the event.
    ///  code="403">The caller does not own this event.
    ///  code="404">The event does not exist.
    [HttpGet("api/events/{eventId:int}/enrolments")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null)
        {
            return NotFound(new { message = "Event does not exist." });
        }

        if (ev.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        var enrolments = await _db.Enrollments
            .Include(en => en.Participant)
            .Include(en => en.Category).ThenInclude(c => c!.Event)
            .Where(en => en.Category!.EventID == eventId)
            .Select(en => ToDto(en))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// Confirms or cancels a participant's enrolment.
    ///  code="200">Returns the updated enrolment.
    ///  code="400">The status value supplied is invalid.
    ///  code="403">The caller may not change this enrolment.
    ///  code="404">The enrolment does not exist.
    [HttpPut("api/enrolments/{id:int}/status")]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusDto dto)
    {
        string[] validStatuses = { "Pending", "Confirmed", "Cancelled" };
        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new { message = "Status must be Pending, Confirmed, or Cancelled." });
        }

        var enrolment = await _db.Enrollments
            .Include(en => en.Participant)
            .Include(en => en.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(en => en.EnrollmentID == id);

        if (enrolment == null || enrolment.Category?.Event == null)
        {
            return NotFound(new { message = "Enrolment does not exist." });
        }

        int currentUserId = this.GetCurrentUserId();
        string? role = this.GetCurrentUserRole();
        bool isOwningOrganiser = role == RoleNames.Organiser && enrolment.Category.Event.OrganiserID == currentUserId;
        bool isOwnParticipantCancelling = role == RoleNames.Participant
            && enrolment.ParticipantID == currentUserId
            && dto.Status == "Cancelled";

        if (!isOwningOrganiser && !isOwnParticipantCancelling)
        {
            return Forbid();
        }

        enrolment.Status = dto.Status;
        await _db.SaveChangesAsync();

        return Ok(ToDto(enrolment));
    }

    private async Task<EnrolmentDto> LoadDto(int enrollmentId)
    {
        var enrolment = await _db.Enrollments
            .Include(en => en.Participant)
            .Include(en => en.Category).ThenInclude(c => c!.Event)
            .FirstAsync(en => en.EnrollmentID == enrollmentId);

        return ToDto(enrolment);
    }

    private static EnrolmentDto ToDto(Enrollment en) => new()
    {
        EnrollmentID = en.EnrollmentID,
        ParticipantID = en.ParticipantID,
        ParticipantName = en.Participant?.FullName ?? string.Empty,
        CategoryID = en.CategoryID,
        CategoryName = en.Category?.CategoryName ?? string.Empty,
        EventID = en.Category?.EventID ?? 0,
        EventName = en.Category?.Event?.EventName ?? string.Empty,
        EnrolmentDate = en.EnrolmentDate,
        Status = en.Status
    };
}