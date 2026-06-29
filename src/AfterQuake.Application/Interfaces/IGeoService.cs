namespace AfterQuake.Application.Interfaces;

public interface IGeoService
{
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    IReadOnlyList<T> FilterByRadius<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double centerLat, double centerLon, double radiusKm);
    IReadOnlyList<T> SortByDistance<T>(IEnumerable<T> items, Func<T, double?> latSelector, Func<T, double?> lonSelector, double originLat, double originLon);
}
