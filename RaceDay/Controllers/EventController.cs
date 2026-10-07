using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Constants;
using RaceDay.Data;
using RaceDay.Dtos;
using RaceDay.Models;

namespace RaceDay.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// Lists all events. Open to everyone, logged in or not.
    ///  code="200">Returns the array of events.
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Include(e => e.Organiser)
            .OrderBy(e => e.EventDate)
            .Select(e => ToDto(e))
            .ToListAsync();

        return Ok(events);
    }

    /// Returns the full detail of a single event.
    /// code="200">Returns the event detail
    ///  code="404">No event exists with that ID.
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event does not exist." });
        }

        return Ok(ToDto(ev));
    }

    /// Creates a new event owned by the logged-in organiser.
    ///  code="201">Returns the newly created event.
    ///  code="400">Validation failed.
    ///  code="403">The caller is not an Organiser.
    [HttpPost]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int organiserId = this.GetCurrentUserId();

        var ev = new Event
        {
            OrganiserID = organiserId,
            EventName = dto.EventName,
            EventType = dto.EventType,
            EventDate = dto.EventDate,
            Province = dto.Province,
            Venue = dto.Venue,
            Description = dto.Description,
            CreatedDate = DateTime.UtcNow
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, ToDto(ev));
    }

    /// Updates an existing event. Only the organiser who owns it may edit it.
    ///  code="200">Returns the updated event.
    /// code="403">The caller does not own this event.
    /// code="404">No event exists with that ID
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event does not exist." });
        }

        if (ev.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        ev.EventName = dto.EventName;
        ev.EventType = dto.EventType;
        ev.EventDate = dto.EventDate;
        ev.Province = dto.Province;
        ev.Venue = dto.Venue;
        ev.Description = dto.Description;

        await _db.SaveChangesAsync();

        return Ok(ToDto(ev));
    }

    /// Deletes an event and its categories. Only the owning organiser may delete it.
    ///  code="200">Event deleted successfully
    ///  code="403">The caller does not own this event
    ///  code="404">No event exists with that ID.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null)
        {
            return NotFound(new { message = "Event does not exist." });
        }

        if (ev.OrganiserID != this.GetCurrentUserId())
        {
            return Forbid();
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Event deleted." });
    }

    private static EventDto ToDto(Event e) => new()
    {
        EventID = e.EventID,
        OrganiserID = e.OrganiserID,
        OrganiserName = e.Organiser?.FullName ?? string.Empty,
        EventName = e.EventName,
        EventType = e.EventType,
        EventDate = e.EventDate,
        Province = e.Province,
        Venue = e.Venue,
        Description = e.Description,
        CreatedDate = e.CreatedDate
    };
}
