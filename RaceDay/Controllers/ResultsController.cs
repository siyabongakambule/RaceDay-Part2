using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;
using RaceDay.Data;
using RaceDay.Dtos;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ResultsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// Captures the finish time and position for a completed enrolment.
    ///  code="201">Returns the newly captured result.
    /// code="403">The caller does not own the event this enrolment belongs to.
    ///  code="404">The enrolment does not exist.</response>
    ///  code="409">A result has already been captured for this enrolment.
    [HttpPost("api/enrolments/{enrollmentId:int}/results")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Capture(int enrollmentId, [FromBody] CreateResultDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var enrolment = await _db.Enrollments
            .Include(en => en.Participant)
            .Include(en => en.Category).ThenInclude(c => c!.Event)
            .Include(en => en.Result)
            .FirstOrDefaultAsync(en => en.EnrollmentID == enrollmentId);

        if (enrolment == null || enrolment.Category?.Event == null)
        {
            return NotFound(new { message = "Enrolment does not exist." });
        }

        int organiserId = this.GetCurrentUserId();
        if (enrolment.Category.Event.OrganiserID != organiserId)
        {
            return Forbid();
        }

        if (enrolment.Result != null)
        {
            return Conflict(new { message = "A result has already been captured for this enrolment." });
        }

        var result = new Result
        {
            EnrollmentID = enrollmentId,
            CapturedByUserID = organiserId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            CapturedDate = DateTime.UtcNow
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByEnrolment), new { enrollmentId }, ToDto(result, enrolment));
    }

    /// Retrieves the result captured for a specific enrolment.
    ///  code="200">Returns the result detail.
    /// code="404">No result has been captured yet for this enrolment.
    [HttpGet("api/enrolments/{enrollmentId:int}/results")]
    [Authorize]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEnrolment(int enrollmentId)
    {
        var result = await _db.Results
            .Include(r => r.Enrollment).ThenInclude(en => en!.Participant)
            .Include(r => r.Enrollment).ThenInclude(en => en!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.EnrollmentID == enrollmentId);

        if (result == null || result.Enrollment == null)
        {
            return NotFound(new { message = "No result has been captured for this enrolment yet." });
        }

        return Ok(ToDto(result, result.Enrollment));
    }

    /// Returns the logged-in participant's full personal race history.
    ///  code="200">Returns the array of the caller's own results.
    [HttpGet("api/users/me/results")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyResults()
    {
        int participantId = this.GetCurrentUserId();

        var results = await _db.Results
            .Include(r => r.Enrollment).ThenInclude(en => en!.Participant)
            .Include(r => r.Enrollment).ThenInclude(en => en!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrollment!.ParticipantID == participantId)
            .Select(r => ToDto(r, r.Enrollment!))
            .ToListAsync();

        return Ok(results);
    }

    /// Returns the full results list for an event, e.g. a leaderboard.
    /// code="200">Returns the array of results ordered by finishing position.
    [HttpGet("api/events/{eventId:int}/results")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var results = await _db.Results
            .Include(r => r.Enrollment).ThenInclude(en => en!.Participant)
            .Include(r => r.Enrollment).ThenInclude(en => en!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrollment!.Category!.EventID == eventId)
            .OrderBy(r => r.Position)
            .Select(r => ToDto(r, r.Enrollment!))
            .ToListAsync();

        return Ok(results);
    }

    private static ResultDto ToDto(Result r, Enrollment en) => new()
    {
        ResultID = r.ResultID,
        EnrollmentID = r.EnrollmentID,
        ParticipantName = en.Participant?.FullName ?? string.Empty,
        EventName = en.Category?.Event?.EventName ?? string.Empty,
        CategoryName = en.Category?.CategoryName ?? string.Empty,
        FinishTime = r.FinishTime,
        Position = r.Position,
        CapturedDate = r.CapturedDate
    };
}
