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
            var events = await _api.GetAsync<List<EventViewModel>>("api/events");
            return View(events ?? new List<EventViewModel>());
        }
        // View the participant's enrolments
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
    }

}