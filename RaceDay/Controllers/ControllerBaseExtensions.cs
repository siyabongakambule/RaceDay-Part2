using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Controllers;

// Small helpers so every controller can read the logged-in user's
// identity out of the JWT claims without repeating the same code.
public static class ControllerBaseExtensions
{
    public static int GetCurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(value ?? "0");
    }

    public static string? GetCurrentUserRole(this ControllerBase controller)
    {
        return controller.User.FindFirstValue(ClaimTypes.Role);
    }
}
