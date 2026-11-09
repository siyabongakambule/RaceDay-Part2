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


        //  CATEGORY MANAGEMENT 

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var raceEvent = await _api.GetAsync<EventViewModel>($"api/events/{eventId}");

            if (raceEvent == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserID != organiserId)
                return Forbid();

            var categories = await _api.GetAsync<List<CategoryViewModel>>(
                $"api/events/{eventId}/categories");

            ViewBag.EventID = eventId;
            ViewBag.EventName = raceEvent.EventName;

            return View(categories ?? new List<CategoryViewModel>());
        }

        [HttpGet]
        public IActionResult CreateCategory(int eventId)
        {
            ViewBag.EventID = eventId;
            return View(new CreateCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            int eventId, CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventID = eventId;
                return View(model);
            }

            var response = await _api.PostAsync(
                $"api/events/{eventId}/categories", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(error)
                ? "Unable to create category."
                : error);

            ViewBag.EventID = eventId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int categoryId)
        {
            var categories = await _api.GetAsync<List<CategoryViewModel>>(
                $"api/events/{eventId}/categories");

            var category = categories?.FirstOrDefault(
                c => c.CategoryID == categoryId);

            if (category == null)
                return NotFound();

            var raceEvent = await _api.GetAsync<EventViewModel>(
                $"api/events/{eventId}");

            if (raceEvent == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserID != organiserId)
                return Forbid();

            ViewBag.EventID = eventId;
            ViewBag.CategoryID = categoryId;

            var model = new CreateCategoryViewModel
            {
                CategoryName = category.CategoryName,
                DistanceKm = category.DistanceKm,
                EntryFee = category.EntryFee,
                MaxParticipants = category.MaxParticipants
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            int eventId, int categoryId, CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventID = eventId;
                ViewBag.CategoryID = categoryId;
                return View(model);
            }

            var response = await _api.PutAsync(
                $"api/categories/{categoryId}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(error)
                ? "Unable to update category."
                : error);

            ViewBag.EventID = eventId;
            ViewBag.CategoryID = categoryId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            var response = await _api.DeleteAsync(
                $"api/categories/{categoryId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(error)
                    ? "Unable to delete category. It may have existing enrolments."
                    : error;
            }

            return RedirectToAction(nameof(Categories), new { eventId });
        }




        public async Task<IActionResult> Enrolments(int eventId)
        {
            var raceEvent = await _api.GetAsync<EventViewModel>(
                $"api/events/{eventId}");

            if (raceEvent == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserID != organiserId)
                return Forbid();

            var enrolments = await _api.GetAsync<List<EnrolmentViewModel>>(
                $"api/events/{eventId}/enrolments");

            ViewBag.EventID = eventId;
            ViewBag.EventName = raceEvent.EventName;

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        // Update an enrolment status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int id, int eventId, string status)
        {
            var response = await _api.PutAsync(
                $"api/enrolments/{id}/status",
                new { Status = status });

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Enrolment status updated successfully!";
            else
                TempData["ErrorMessage"] = "Unable to update enrolment status.";

            return RedirectToAction(nameof(Enrolments), new { eventId });
        }
    
    public async Task<IActionResult> Results(int eventId)
        {
            var raceEvent = await _api.GetAsync<EventViewModel>(
                $"api/events/{eventId}");

            if (raceEvent == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId) ||
                raceEvent.OrganiserID != organiserId)
                return Forbid();

            var enrolments = await _api.GetAsync<List<EnrolmentViewModel>>(
                $"api/events/{eventId}/enrolments");

            var results = await _api.GetAsync<List<ResultViewModel>>(
                $"api/events/{eventId}/results");

            var model = new EventResultsViewModel
            {
                EventID = eventId,
                EventName = raceEvent.EventName,
                Enrolments = enrolments ?? new List<EnrolmentViewModel>(),
                Results = results ?? new List<ResultViewModel>()
            };

            return View(model);
        }

        // Record a participant's race result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            int eventId,
            int enrollmentId,
            TimeSpan finishTime,
            int? position)
        {
            var response = await _api.PostAsync(
                $"api/enrolments/{enrollmentId}/results",
                new
                {
                    FinishTime = finishTime,
                    Position = position
                });

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Race result recorded successfully!";
            else
                TempData["ErrorMessage"] = "Unable to record the race result.";

            return RedirectToAction(nameof(Results), new { eventId });
        }
    }
}