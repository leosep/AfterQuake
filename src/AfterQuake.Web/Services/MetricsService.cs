using System.Collections.Concurrent;

namespace AfterQuake.Web.Services;

public class MetricsService
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, double> _gauges = new();

    public void Increment(string name, long value = 1) =>
        _counters.AddOrUpdate(name, value, (_, v) => v + value);

    public void Gauge(string name, double value) =>
        _gauges[name] = value;

    public Dictionary<string, object> Snapshot()
    {
        var result = new Dictionary<string, object>();
        foreach (var kvp in _counters) result[$"counter_{kvp.Key}"] = kvp.Value;
        foreach (var kvp in _gauges) result[$"gauge_{kvp.Key}"] = kvp.Value;
        return result;
    }

    public string PrometheusFormat()
    {
        var lines = new List<string>();
        foreach (var kvp in _counters)
            lines.Add($"# HELP afterquake_{kvp.Key} Counter\n# TYPE afterquake_{kvp.Key} counter\nafterquake_{kvp.Key} {kvp.Value}");
        foreach (var kvp in _gauges)
            lines.Add($"# HELP afterquake_{kvp.Key} Gauge\n# TYPE afterquake_{kvp.Key} gauge\nafterquake_{kvp.Key} {kvp.Value}");
        return string.Join("\n", lines);
    }
}
