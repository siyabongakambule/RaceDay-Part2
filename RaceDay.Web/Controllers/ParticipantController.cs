using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // Explore available events
        public async Task<IActionResult> Events()
        {
            var events = await _api.GetAsync<List<EventViewModel>>(
                "api/events");

            return View(events ?? new List<EventViewModel>());
        }

        // View categories belonging to an event
        public async Task<IActionResult> Categories(int eventId)
        {
            var raceEvent = await _api.GetAsync<EventViewModel>(
                $"api/events/{eventId}");

            if (raceEvent == null)
                return NotFound();

            var categories = await _api.GetAsync<List<CategoryViewModel>>(
                $"api/events/{eventId}/categories");

            ViewBag.EventID = eventId;
            ViewBag.EventName = raceEvent.EventName;

            return View(categories ?? new List<CategoryViewModel>());
        }

        // Enrol in an event category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {
            var response = await _api.PostAsync<object>(
                $"api/categories/{categoryId}/enrol",
                new { });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "You have successfully enrolled in this category!";

                return RedirectToAction(nameof(Enrolments));
            }

            var error = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(error)
                ? "Unable to complete your enrolment. Please try again."
                : error;

            return RedirectToAction(nameof(Categories),
                new { eventId });
        }

        // View participant enrolments
        public async Task<IActionResult> Enrolments()
        {
            var enrolments = await _api.GetAsync<List<EnrolmentViewModel>>(
                "api/users/me/enrolments");

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        // View participant results
        public async Task<IActionResult> Results()
        {
            var results = await _api.GetAsync<List<ResultViewModel>>(
                "api/users/me/results");

            return View(results ?? new List<ResultViewModel>());
        }
    
    public async Task<IActionResult> Profile()
        {
            var profile = await _api.GetAsync<ProfileViewModel>("api/users/me");

            if (profile == null)
                return View(new ProfileViewModel());

            return View(profile);
        }

        // Update participant profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _api.PutAsync(
                "api/users/me",
                new
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber
                });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            TempData["ErrorMessage"] = "Unable to update your profile.";
            return View(model);
        }
    }
}
