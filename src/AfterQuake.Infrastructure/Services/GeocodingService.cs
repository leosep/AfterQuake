using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AfterQuake.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AfterQuake.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private DateTime _lastRequestTime = DateTime.MinValue;
    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public GeocodingService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var cacheKey = $"geo_{latitude:F6}_{longitude:F6}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        await EnforceRateLimitAsync();

        var response = await _httpClient.GetAsync($"/reverse?lat={latitude}&lon={longitude}&format=json&addressdetails=1");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NominatimResponse>();
        var address = result?.DisplayName;

        if (address is not null)
            _cache.Set(cacheKey, address, CacheDuration);

        return address;
    }

    public async Task<string?> GetZoneCodeAsync(double latitude, double longitude)
    {
        var cacheKey = $"zone_{latitude:F6}_{longitude:F6}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        await EnforceRateLimitAsync();

        var response = await _httpClient.GetAsync($"/reverse?lat={latitude}&lon={longitude}&format=json&addressdetails=1");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NominatimResponse>();
        var zone = result?.Address?.Postcode ?? result?.Address?.Suburb ?? result?.Address?.CityDistrict;

        if (zone is not null)
            _cache.Set(cacheKey, zone, CacheDuration);

        return zone;
    }

    private async Task EnforceRateLimitAsync()
    {
        var elapsed = DateTime.UtcNow - _lastRequestTime;
        if (elapsed < MinRequestInterval)
            await Task.Delay(MinRequestInterval - elapsed);
        _lastRequestTime = DateTime.UtcNow;
    }

    private class NominatimResponse
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        public string? Postcode { get; set; }
        public string? Suburb { get; set; }

        [JsonPropertyName("city_district")]
        public string? CityDistrict { get; set; }
    }
}
