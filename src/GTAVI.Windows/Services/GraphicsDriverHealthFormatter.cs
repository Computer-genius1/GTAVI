using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public static class GraphicsDriverHealthFormatter
{
    private static readonly IReadOnlyDictionary<int, string> DeviceErrorDescriptions =
        new Dictionary<int, string>
        {
            [0] = "Windows reports that the device is working properly.",
            [1] = "The device is not configured correctly.",
            [2] = "Windows cannot load the driver.",
            [3] = "The driver may be corrupted or system memory may be low.",
            [4] = "The device is not working properly.",
            [10] = "Windows cannot start the device.",
            [12] = "The device cannot find enough free resources.",
            [14] = "The device requires a restart.",
            [18] = "Windows recommends reinstalling the device drivers.",
            [19] = "A configuration registry problem was detected.",
            [22] = "The device is disabled.",
            [24] = "The device is not present, is not working properly, or does not have all its drivers installed.",
            [28] = "The device drivers are not installed.",
            [29] = "The device is disabled because its firmware did not provide the required resources.",
            [31] = "Windows cannot load the drivers required for this device.",
            [32] = "A driver service for this device is disabled.",
            [37] = "The device driver failed to initialize.",
            [39] = "Windows cannot load the device driver.",
            [40] = "Windows cannot access the device because its service key information is missing or recorded incorrectly.",
            [43] = "Windows stopped the device because it reported problems.",
            [45] = "The device is not currently connected.",
            [48] = "The device driver has been blocked from starting.",
            [49] = "Windows cannot start the device because the system hive is too large."
        };

    public static string Format(GraphicsDriverHealthReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.Adapters.Count == 0)
            return string.IsNullOrWhiteSpace(report.CollectionStatus)
                ? "No graphics adapters were returned by Windows."
                : report.CollectionStatus;

        var lines = new List<string>();

        foreach (var adapter in report.Adapters)
        {
            var name = string.IsNullOrWhiteSpace(adapter.AdapterName)
                ? "Unnamed graphics adapter"
                : adapter.AdapterName.Trim();

            lines.Add(name);
            lines.Add($"  Device status: {FormatDeviceStatus(adapter)}");
            lines.Add($"  Driver version: {FormatDriverField(adapter.DriverVersion)}");
            lines.Add($"  Driver date: {FormatDriverField(adapter.DriverDate)}");
        }

        if (!string.IsNullOrWhiteSpace(report.CollectionStatus))
            lines.Add($"Inventory note: {report.CollectionStatus}");

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatDeviceStatus(GraphicsDriverHealthInfo adapter)
    {
        if (adapter.ConfigurationManagerErrorCode is int code)
        {
            var description = DeviceErrorDescriptions.TryGetValue(code, out var known)
                ? known
                : "Windows reported a device configuration error; consult Device Manager for details.";

            return code == 0
                ? $"OK (Code 0) — {description}"
                : $"Attention required (Code {code}) — {description}";
        }

        return string.IsNullOrWhiteSpace(adapter.WindowsStatus)
            ? "Unknown — Windows did not provide a device status or configuration code."
            : $"Unverified — Windows status: {adapter.WindowsStatus.Trim()}; no configuration code was provided.";
    }

    private static string FormatDriverField(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Not reported by Windows" : value.Trim();
}
