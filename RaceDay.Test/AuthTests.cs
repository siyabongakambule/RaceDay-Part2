using System.Net;
using System.Net.Http.Json;
using RaceDay.Dtos;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidOrganiserDetails_ReturnsCreated()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Thabo Mokoena",
            Email = "thabo.register.test@-tests.com",
            Password = "StrongPassword123!",
            PhoneNumber = "0821234567",
            Role = "Organiser"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.Equal("Organiser", profile!.Role);
        Assert.Equal(dto.Email, profile.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Amahle Dlamini",
            Email = "amahle.duplicate.test@-tests.com",
            Password = "StrongPassword123!",
            Role = "Participant"
        };

        var first = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Sneaky Admin",
            Email = "sneaky.admin.test@-tests.com",
            Password = "StrongPassword123!",
            Role = "Admin" // Not allowed at self-registration
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsTokenAndProfile()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Lerato Nkosi",
            Email = "lerato.login.test@-tests.com",
            Password = "StrongPassword123!",
            Role = "Participant"
        };
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var login = new LoginDto { Email = dto.Email, Password = dto.Password };
        var response = await client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("Participant", body.User.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Johan van Wyk",
            Email = "johan.wrongpass.test@-tests.com",
            Password = "StrongPassword123!",
            Role = "Participant"
        };
        await client.PostAsJsonAsync("/api/auth/register", dto);

        var login = new LoginDto { Email = dto.Email, Password = "TotallyWrongPassword" };
        var response = await client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
