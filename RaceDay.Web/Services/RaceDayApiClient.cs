using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.Web.Services;

public class RaceDayApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RaceDayApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // ATTACH JWT USING SESSION
    private void AddAuthorizationHeader()
    {
        var context = _httpContextAccessor.HttpContext;

        var token = context?.Session.GetString("AccessToken");

        // Fallback to authentication claim
        if (string.IsNullOrWhiteSpace(token))
        {
            token = context?.User.FindFirst("AccessToken")?.Value;
        }

        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // REGISTER
    public async Task<HttpResponseMessage> RegisterAsync<T>(T data)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/auth/register", data);
    }

    // LOGIN
    public async Task<T?> LoginAsync<T>(object data)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login", data);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    // GET
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    // POST
    public async Task<HttpResponseMessage> PostAsync<T>(
        string endpoint, T data)
    {
        AddAuthorizationHeader();

        return await _httpClient.PostAsJsonAsync(endpoint, data);
    }

    // PUT
    public async Task<HttpResponseMessage> PutAsync<T>(
        string endpoint, T data)
    {
        AddAuthorizationHeader();

        return await _httpClient.PutAsJsonAsync(endpoint, data);
    }

    // DELETE
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        AddAuthorizationHeader();

        return await _httpClient.DeleteAsync(endpoint);
    }
}