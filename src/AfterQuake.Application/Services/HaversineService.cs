using AfterQuake.Application.Interfaces;

namespace AfterQuake.Application.Services;

public class HaversineService : IGeoService
{
    private const double EarthRadiusKm = 6371.0;

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    public IReadOnlyList<T> FilterByRadius<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double centerLat, double centerLon, double radiusKm)
    {
        return items.Where(item =>
        {
            var lat = latSelector(item);
            var lon = lonSelector(item);
            if (lat is null || lon is null) return false;
            return CalculateDistance(centerLat, centerLon, lat.Value, lon.Value) <= radiusKm;
        }).ToList();
    }

    public IReadOnlyList<T> SortByDistance<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double originLat, double originLon)
    {
        return items
            .Where(item => latSelector(item) is not null && lonSelector(item) is not null)
            .OrderBy(item => CalculateDistance(originLat, originLon, latSelector(item)!.Value, lonSelector(item)!.Value))
            .ToList();
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
