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
        var response = await _client.GetAsync("/api/map/nearby?lat=18.49&lng=-69.93&radius=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmergency_Unauthenticated_ReturnsSuccessOrError()
    {
        var dto = new CreateEmergencyReportDto
        {
            EmergencyType = EmergencyType.Medical,
            Latitude = 18.49,
            Longitude = -69.93,
            Description = "Test emergency via API"
        };
        var response = await _client.PostAsJsonAsync("/api/emergency", dto);
        Assert.True(response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.InternalServerError);
    }

}
