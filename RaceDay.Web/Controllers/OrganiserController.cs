using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }
}
