using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public static class GraphicsApiCapabilityFormatter
{
    public static string Format(GraphicsApiCapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        var direct3D11 = capabilities.Direct3D11Available
            ? $"Available (Feature Level {NormalizeFeatureLevel(capabilities.Direct3D11FeatureLevel)})"
            : $"Unavailable ({NormalizeStatus(capabilities.Direct3D11Status)})";

        var direct3D12 = capabilities.Direct3D12Available
            ? "Available"
            : $"Unavailable ({NormalizeStatus(capabilities.Direct3D12Status)})";

        return $"Direct3D 11: {direct3D11}{Environment.NewLine}Direct3D 12: {direct3D12}";
    }

    private static string NormalizeFeatureLevel(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();

    private static string NormalizeStatus(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "No additional details" : value.Trim();
}
