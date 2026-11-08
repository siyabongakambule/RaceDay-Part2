using Microsoft.AspNetCore.Authentication.Cookies;

using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC

builder.Services.AddControllersWithViews();

// Access the current user's authentication information

builder.Services.AddHttpContextAccessor();

// Cookie authentication

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>

    {

        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "RaceDay.Auth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);

        options.SlidingExpiration = true;

    });

// Authorization

builder.Services.AddAuthorization();

// Register the RaceDay API service

builder.Services.AddHttpClient<RaceDayApiClient>(client =>

{

    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiUrl))

    {

        throw new InvalidOperationException(

            "ApiSettings:BaseUrl is missing from appsettings.json.");

    }

    client.BaseAddress = new Uri(apiUrl);

    client.Timeout = TimeSpan.FromSeconds(30);

});

var app = builder.Build();

// HTTP pipeline

if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Important: authentication must come before authorization

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();