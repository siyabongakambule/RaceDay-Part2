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

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Events()
        {
            var events = await _api.GetAsync<List<EventViewModel>>("api/events");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Forbid();

            var myEvents = (events ?? new List<EventViewModel>())
                .Where(e => e.OrganiserID == organiserId)
                .ToList();

            return View(myEvents);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new CreateEventViewModel
            {
                EventDate = DateTime.Today.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _api.PostAsync("api/events", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your event was created successfully!";
                return RedirectToAction(nameof(Events));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                ? "Unable to create the event. Please try again."
                : errorMessage);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var raceEvent = await _api.GetAsync<EventViewModel>($"api/events/{id}");

            if (raceEvent == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserID != organiserId)
                return Forbid();

            var model = new CreateEventViewModel
            {
                EventName = raceEvent.EventName,
                EventType = raceEvent.EventType,
                EventDate = raceEvent.EventDate,
                Province = raceEvent.Province,
                Venue = raceEvent.Venue,
                Description = raceEvent.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _api.PutAsync($"api/events/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your event was updated successfully!";
                return RedirectToAction(nameof(Events));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(errorMessage)
                ? "Unable to update the event."
                : errorMessage);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var response = await _api.DeleteAsync($"api/events/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "The event could not be deleted. It may have existing records or you may not have permission.";
            }

            return RedirectToAction(nameof(Events));
        }
    }
}