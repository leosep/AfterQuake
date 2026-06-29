namespace AfterQuake.Application.Interfaces;

public interface IGeocodingService
{
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
    Task<string?> GetZoneCodeAsync(double latitude, double longitude);
}
