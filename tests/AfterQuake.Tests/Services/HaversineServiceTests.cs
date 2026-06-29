using AfterQuake.Application.Interfaces;
using AfterQuake.Infrastructure.Services;

namespace AfterQuake.Tests.Services;

public class HaversineServiceTests
{
    private readonly IHaversineService _service = new HaversineService();

    [Fact]
    public void CalculateDistance_ShouldReturnApproximately115KmBetweenSantiagoAndValparaiso()
    {
        var santiagoLat = -33.4489;
        var santiagoLng = -70.6693;
        var valparaisoLat = -33.0472;
        var valparaisoLng = -71.6127;

        var distance = _service.CalculateDistance(santiagoLat, santiagoLng, valparaisoLat, valparaisoLng);

        Assert.True(distance > 85 && distance < 110,
            $"Expected ~98km but got {distance}km");
    }

    [Fact]
    public void IsWithinRadius_ShouldReturnTrueWhenPointsAreWithinGivenRadius()
    {
        var centerLat = -33.4489;
        var centerLng = -70.6693;
        var nearbyLat = -33.4500;
        var nearbyLng = -70.6700;

        var result = _service.IsWithinRadius(centerLat, centerLng, nearbyLat, nearbyLng, radiusKm: 1);

        Assert.True(result);
    }

    [Fact]
    public void IsWithinRadius_ShouldReturnFalseWhenPointsAreOutsideGivenRadius()
    {
        var centerLat = -33.4489;
        var centerLng = -70.6693;
        var farLat = -33.0472;
        var farLng = -71.6127;

        var result = _service.IsWithinRadius(centerLat, centerLng, farLat, farLng, radiusKm: 10);

        Assert.False(result);
    }

    [Fact]
    public void CalculateDistance_ShouldReturnZeroForSamePoint()
    {
        var lat = -33.4489;
        var lng = -70.6693;

        var distance = _service.CalculateDistance(lat, lng, lat, lng);

        Assert.Equal(0, distance);
    }
}
