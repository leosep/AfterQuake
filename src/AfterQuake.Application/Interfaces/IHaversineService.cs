namespace AfterQuake.Application.Interfaces;

public interface IHaversineService
{
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, double radiusKm);
}
