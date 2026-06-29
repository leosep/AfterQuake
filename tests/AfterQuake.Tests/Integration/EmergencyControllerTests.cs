using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using AfterQuake.Application.DTOs;
using AfterQuake.Domain.Enumerations;
using Xunit;

namespace AfterQuake.Tests.Integration;

[Collection("Integration Testing")]
public class EmergencyControllerTests
{
    private readonly HttpClient _client;

    public EmergencyControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EmergencyReport_Get_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Emergency/Report");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EmergencyIndex_ReturnsRedirectToLogin()
    {
        var response = await _client.GetAsync("/Emergency");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task PersonSearch_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Person");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HelpRequestCreate_Get_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/HelpRequest/Create");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GuideIndex_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Guide");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DirectoryIndex_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Directory");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShelterMap_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Shelter/Map");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DonationIndex_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Donation");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShelterList_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Shelter");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HospitalPatients_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Person/HospitalPatients");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
