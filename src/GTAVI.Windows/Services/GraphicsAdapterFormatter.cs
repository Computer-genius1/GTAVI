using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public static class GraphicsAdapterFormatter
{
    public static string Format(IEnumerable<GraphicsAdapterInfo> adapters)
    {
        var entries = adapters.ToArray();
        if (entries.Length == 0)
            return "No graphics adapters reported by Windows.";

        return string.Join(Environment.NewLine + Environment.NewLine,
            entries.Select((adapter, index) =>
                $"{index + 1}. {ValueOrUnavailable(adapter.Name)}" + Environment.NewLine +
                $"   Driver: {ValueOrUnavailable(adapter.DriverVersion)}" +
                (string.IsNullOrWhiteSpace(adapter.DriverDate) ? "" : $" ({adapter.DriverDate})") + Environment.NewLine +
                $"   Video processor: {ValueOrUnavailable(adapter.VideoProcessor)}" + Environment.NewLine +
                $"   Adapter memory reported by Windows: {ValueOrUnavailable(adapter.AdapterMemory)}" + Environment.NewLine +
                $"   Current display mode: {ValueOrUnavailable(adapter.DisplayResolution)}"));
    }

    private static string ValueOrUnavailable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unavailable" : value;
}
