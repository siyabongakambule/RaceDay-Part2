using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        // Organiser dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // Display events belonging to the logged-in organiser
        public async Task<IActionResult> Events()
        {
            var events = await _api.GetAsync<List<EventViewModel>>(
                "api/events");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Forbid();
            }

            var myEvents = (events ?? new List<EventViewModel>())
                .Where(e => e.OrganiserID == organiserId)
                .ToList();

            return View(myEvents);
        }
    }
}