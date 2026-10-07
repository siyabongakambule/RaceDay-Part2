using System.Net;
using System.Net.Http.Json;
using RaceDay.Dtos;
using Xunit;

namespace RaceDay.Tests;

public class EventsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public EventsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static CreateEventDto SampleEvent() => new()
    {
        EventName = "Test Park Run",
        EventType = "Run",
        EventDate = DateTime.UtcNow.AddDays(30),
        Province = "Gauteng",
        Venue = "Test Track",
        Description = "A test event created by an automated test."
    };

    [Fact]
    public async Task Create_AsOrganiser_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        string token = await TestAuthHelper.RegisterAndLoginAsync(client, "Organiser", "create-201");
        TestAuthHelper.SetBearerToken(client, token);

        var response = await client.PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.Equal("Test Park Run", created!.EventName);
    }

    [Fact]
    public async Task Create_AsParticipant_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        string token = await TestAuthHelper.RegisterAndLoginAsync(client, "Participant", "create-403");
        TestAuthHelper.SetBearerToken(client, token);

        var response = await client.PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events", SampleEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ByNonOwningOrganiser_ReturnsForbidden()
    {
        var client = _factory.CreateClient();

        string ownerToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Organiser", "owner-update");
        TestAuthHelper.SetBearerToken(client, ownerToken);
        var createResponse = await client.PostAsJsonAsync("/api/events", SampleEvent());
        var created = await createResponse.Content.ReadFromJsonAsync<EventDto>();

        string otherToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Organiser", "other-update");
        TestAuthHelper.SetBearerToken(client, otherToken);

        var updateResponse = await client.PutAsJsonAsync($"/api/events/{created!.EventID}", SampleEvent());

        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }
}
