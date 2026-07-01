using AfterQuake.Application.Interfaces;
using AfterQuake.Infrastructure.Services;

namespace AfterQuake.Tests.Services;

public class HaversineServiceTests
{
    private readonly IHaversineService _service = new HaversineService();

    [Fact]
    public void CalculateDistance_ShouldReturnApproximately38KmBetweenSantiagoDRAndPuertoPlata()
    {
        var santiagoLat = 19.4517;
        var santiagoLng = -70.6970;
        var puertoPlataLat = 19.7936;
        var puertoPlataLng = -70.6891;

        var distance = _service.CalculateDistance(santiagoLat, santiagoLng, puertoPlataLat, puertoPlataLng);

        Assert.True(distance > 30 && distance < 50,
            $"Expected ~38km but got {distance}km");
    }

    [Fact]
    public void IsWithinRadius_ShouldReturnTrueWhenPointsAreWithinGivenRadius()
    {
        var centerLat = 18.4861;
        var centerLng = -69.9312;
        var nearbyLat = 18.4870;
        var nearbyLng = -69.9320;

        var result = _service.IsWithinRadius(centerLat, centerLng, nearbyLat, nearbyLng, radiusKm: 1);

        Assert.True(result);
    }

    [Fact]
    public void IsWithinRadius_ShouldReturnFalseWhenPointsAreOutsideGivenRadius()
    {
        var centerLat = 18.4861;
        var centerLng = -69.9312;
        var farLat = 19.4517;
        var farLng = -70.6970;

        var result = _service.IsWithinRadius(centerLat, centerLng, farLat, farLng, radiusKm: 10);

        Assert.False(result);
    }

    [Fact]
    public void CalculateDistance_ShouldReturnZeroForSamePoint()
    {
        var lat = 18.4861;
        var lng = -69.9312;

        var distance = _service.CalculateDistance(lat, lng, lat, lng);

        Assert.Equal(0, distance);
    }
}
