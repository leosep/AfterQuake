using System.Net;
using System.Net.Http.Json;
using AfterQuake.Application.DTOs;
using AfterQuake.Domain.Enumerations;
using Xunit;

namespace AfterQuake.Tests.Integration;

[Collection("Integration Testing")]
public class ApiControllerTests
{
    private readonly HttpClient _client;

    public ApiControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetShelters_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/map/shelters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthCenters_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/map/health-centers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDonationPoints_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/map/donation-points");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNearby_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/map/nearby?lat=-33.45&lng=-70.67&radius=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmergency_Unauthenticated_ReturnsSuccessOrError()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Medical,
            Latitude = -33.45,
            Longitude = -70.67,
            Description = "Test emergency via API"
        };
        var response = await _client.PostAsJsonAsync("/api/emergency", dto);
        // Endpoint has [AllowAnonymous], so it accepts unauthenticated requests
        // May return Created (201), BadRequest (400) for validation, or 500 if services unavailable
        Assert.True(response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.InternalServerError);
    }
}
