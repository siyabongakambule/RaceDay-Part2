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

        // Display the create event form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new CreateEventViewModel
            {
                EventDate = DateTime.Today.AddDays(1)
            });
        }

        // Submit a new event to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _api.PostAsync(
                "api/events", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your event was created successfully!";
                return RedirectToAction(nameof(Events));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to create the event. Please try again."
                    : errorMessage);

            return View(model);
        }
    }
}