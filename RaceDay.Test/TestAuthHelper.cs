using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Dtos;

namespace RaceDay.Tests;

public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string role, string emailSuffix)
    {
        var register = new RegisterDto
        {
            FullName = $"Test {role} {emailSuffix}",
            Email = $"{role.ToLower()}.{emailSuffix}@tests.com",
            Password = "TestPassword123!",
            PhoneNumber = "0800000000",
            Role = role
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", register);

if (!registerResponse.IsSuccessStatusCode)
{
    var error = await registerResponse.Content.ReadAsStringAsync();

    throw new Exception(
        $"Registration failed with status {(int)registerResponse.StatusCode}: {error}");
}

        var login = new LoginDto
        {
            Email = register.Email,
            Password = register.Password
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.Token;
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
