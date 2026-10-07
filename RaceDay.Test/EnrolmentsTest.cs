using System.Net;
using System.Net.Http.Json;
using RaceDay.Dtos;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public EnrolmentsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    // Helper: organiser creates an event and one category, returns the category ID.
    private async Task<int> CreateEventWithCategoryAsync(HttpClient client, string organiserSuffix)
    {
        string organiserToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Organiser", organiserSuffix);
        TestAuthHelper.SetBearerToken(client, organiserToken);

        var eventDto = new CreateEventDto
        {
            EventName = "Enrolment Test Event",
            EventType = "Run",
            EventDate = DateTime.UtcNow.AddDays(20),
            Province = "Western Cape",
            Venue = "Test Venue"
        };
        var eventResponse = await client.PostAsJsonAsync("/api/events", eventDto);
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<EventDto>();

        var categoryDto = new CreateCategoryDto
        {
            CategoryName = "10km",
            DistanceKm = 10,
            EntryFee = 100,
            MaxParticipants = 500
        };
        var categoryResponse = await client.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventID}/categories", categoryDto);
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        client.DefaultRequestHeaders.Authorization = null;
        return createdCategory!.CategoryID;
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreatedAndIsRecorded()
    {
        var client = _factory.CreateClient();
        int categoryId = await CreateEventWithCategoryAsync(client, "enrol-201");

        string participantToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Participant", "enrol-201");
        TestAuthHelper.SetBearerToken(client, participantToken);

        var enrolResponse = await client.PostAsync($"/api/categories/{categoryId}/enrol", null);
        Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);

        var myEnrolments = await client.GetFromJsonAsync<List<EnrolmentDto>>("/api/users/me/enrolments");
        Assert.Contains(myEnrolments!, e => e.CategoryID == categoryId && e.Status == "Pending");
    }

    [Fact]
    public async Task Enrol_AsOrganiser_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        int categoryId = await CreateEventWithCategoryAsync(client, "enrol-403");

        string organiserToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Organiser", "enrol-403-attempt");
        TestAuthHelper.SetBearerToken(client, organiserToken);

        var response = await client.PostAsync($"/api/categories/{categoryId}/enrol", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_Twice_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        int categoryId = await CreateEventWithCategoryAsync(client, "enrol-409");

        string participantToken = await TestAuthHelper.RegisterAndLoginAsync(client, "Participant", "enrol-409");
        TestAuthHelper.SetBearerToken(client, participantToken);

        var first = await client.PostAsync($"/api/categories/{categoryId}/enrol", null);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsync($"/api/categories/{categoryId}/enrol", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        int categoryId = await CreateEventWithCategoryAsync(client, "enrol-401");

        var response = await client.PostAsync($"/api/categories/{categoryId}/enrol", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
