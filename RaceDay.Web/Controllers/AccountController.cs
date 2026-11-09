using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;

    public AccountController(RaceDayApiClient api)
    {
        _api = api;
    }

    // LOGIN PAGE
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // PROCESS LOGIN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var loginData = new
        {
            model.Email,
            model.Password
        };

        var response = await _api.LoginAsync<AuthResponse>(loginData);

        if (response == null || string.IsNullOrWhiteSpace(response.Token))
        {
            ModelState.AddModelError(
                string.Empty, "Invalid email or password.");

            return View(model);
        }

        // STORE USER DETAILS IN SESSION
        HttpContext.Session.SetString("AccessToken", response.Token);
        HttpContext.Session.SetString("UserRole", response.User.Role);
        HttpContext.Session.SetString("UserName", response.User.FullName);
        HttpContext.Session.SetString(
            "UserID", response.User.UserID.ToString());

        // CREATE AUTHENTICATION CLAIMS
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                response.User.UserID.ToString()),

            new Claim(
                ClaimTypes.Name,
                response.User.FullName),

            new Claim(
                ClaimTypes.Email,
                response.User.Email),

            new Claim(
                ClaimTypes.Role,
                response.User.Role),

            new Claim(
                "AccessToken",
                response.Token)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        // REDIRECT ACCORDING TO ROLE
        if (response.User.Role == "Organiser")
            return RedirectToAction("Dashboard", "Organiser");

        if (response.User.Role == "Participant")
            return RedirectToAction("Dashboard", "Participant");

        return RedirectToAction("Index", "Home");
    }

    // REGISTRATION PAGE
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // PROCESS REGISTRATION
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var registerData = new
        {
            model.FullName,
            model.Email,
            model.Password,
            model.PhoneNumber,
            model.Role
        };

        var result = await _api.RegisterAsync(registerData);

        if (!result.IsSuccessStatusCode)
        {
            var error = await result.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                "Registration failed. Please check your details. " + error);

            return View(model);
        }

        TempData["Success"] =
            "Registration successful. You can now log in.";

        return RedirectToAction("Login");
    }

    // LOGOUT
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // CLEAR SESSION DATA
        HttpContext.Session.Clear();

        // SIGN OUT AUTHENTICATED USER
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    // ACCESS DENIED
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

// API LOGIN RESPONSE MODELS
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public UserProfile User { get; set; } = new();
}

public class UserProfile
{
    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}