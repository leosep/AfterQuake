using AfterQuake.Application.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class HaversineService : IHaversineService, IGeoService
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

    public bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, double radiusKm)
    {
        return CalculateDistance(lat1, lon1, lat2, lon2) <= radiusKm;
    }

    public IReadOnlyList<T> FilterByRadius<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double centerLat, double centerLon, double radiusKm)
    {
        return items.Where(item =>
        {
            var lat = latSelector(item);
            var lon = lonSelector(item);
            return lat.HasValue && lon.HasValue && CalculateDistance(centerLat, centerLon, lat.Value, lon.Value) <= radiusKm;
        }).ToList();
    }

    public IReadOnlyList<T> SortByDistance<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double originLat, double originLon)
    {
        return items.OrderBy(item =>
        {
            var lat = latSelector(item);
            var lon = lonSelector(item);
            return lat.HasValue && lon.HasValue ? CalculateDistance(originLat, originLon, lat.Value, lon.Value) : double.MaxValue;
        }).ToList();
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;
}
