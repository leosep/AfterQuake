using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AfterQuake.Tests.Integration;

[Collection("Integration Testing")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@afterquake.com",
            Password = "AfterQuake2024!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(content);
        Assert.True(content.ContainsKey("token"));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@afterquake.com",
            Password = "wrongpassword"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_CreatesUser()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = $"newuser{Guid.NewGuid():N}@test.com",
            Password = "NewUser1234!",
            FullName = "New Test User"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
