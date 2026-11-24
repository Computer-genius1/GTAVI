using System.Management;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public sealed class SystemInfoService
{
    public async Task<IReadOnlyList<DiagnosticItem>> CollectAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var graphics = CollectGraphicsAdapters();
            var items = new List<DiagnosticItem>
            {
                new("Operating system", GetWindowsName(), "Detected from the Windows registry."),
                new("System architecture", RuntimeInformation.OSArchitecture.ToString(),
                    $"Process architecture: {RuntimeInformation.ProcessArchitecture}"),
                new("Processor", GetWmiValue("SELECT Name FROM Win32_Processor", "Name"),
                    GetProcessorDetails()),
                new("Installed memory", GetMemory(), GetMemoryDetails()),
                new("Graphics adapter count", graphics.Count.ToString(),
                    graphics.Count > 1
                        ? "Multiple adapters are visible to Windows. This inventory does not assume which GPU a game would select."
                        : "Count of display adapters returned by Windows WMI."),
                new("Graphics adapters", GraphicsAdapterFormatter.Format(graphics),
                    "Adapter vendors are identified where possible. Windows WMI alone does not reliably establish whether a GPU is integrated or dedicated; no role is guessed. Adapter memory can be capped or rounded by drivers."),
                new("Graphics driver health",
                    GraphicsDriverHealthFormatter.Format(new GraphicsDriverHealthService().Collect()),
                    "Uses Windows Plug and Play status codes and reported driver metadata. Missing driver fields are flagged as unreported, not automatically treated as a fault; no driver updates or repairs are performed."),
                new("DirectX", GetRegistryValue(Registry.LocalMachine,
                    @"SOFTWARE\Microsoft\DirectX", "Version"), "Version reported by the DirectX registry key; this is not a feature-level test."),
                new("Graphics API capabilities",
                    GraphicsApiCapabilityFormatter.Format(new GraphicsApiCapabilityService().Detect()),
                    "Tests Direct3D 11 and 12 device creation on the default hardware adapter only. It does not test every listed GPU or establish game compatibility."),
                new("System manufacturer", GetWmiValue("SELECT Manufacturer FROM Win32_ComputerSystem", "Manufacturer")),
                new("System model", GetWmiValue("SELECT Model FROM Win32_ComputerSystem", "Model")),
                new("BIOS version", GetWmiValue("SELECT SMBIOSBIOSVersion FROM Win32_BIOS", "SMBIOSBIOSVersion")),
                new("Available storage", GetDriveSummary(), "Ready local fixed volumes, total capacity, and available free space.")
            };

            cancellationToken.ThrowIfCancellationRequested();
            return (IReadOnlyList<DiagnosticItem>)items;
        }, cancellationToken);
    }

    private static string GetWindowsName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (key is null) return "Unable to read Windows version";
            var product = key.GetValue("ProductName")?.ToString() ?? "Windows";
            var display = key.GetValue("DisplayVersion")?.ToString();
            var build = key.GetValue("CurrentBuildNumber")?.ToString();
            var ubr = key.GetValue("UBR")?.ToString();
            var buildText = string.IsNullOrWhiteSpace(build) ? "" : $" (build {build}{(string.IsNullOrWhiteSpace(ubr) ? "" : "." + ubr)})";
            return $"{product}{(string.IsNullOrWhiteSpace(display) ? "" : " " + display)}{buildText}";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            DiagnosticLogService.Write("Read Windows version", ex);
            return "Access denied or unavailable";
        }
    }

    private static string GetMemory()
    {
        var raw = GetWmiRawValue("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory");
        if (ulong.TryParse(raw, out var bytes) && bytes > 0)
            return $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
        return "Unavailable";
    }

    private static string GetMemoryDetails()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Capacity, Speed, Manufacturer, PartNumber FROM Win32_PhysicalMemory");
            using var results = searcher.Get();
            var modules = results.Cast<ManagementObject>().ToArray();
            try
            {
                var total = modules.Aggregate(0UL, (sum, module) =>
                {
                    var capacity = TryUInt64(module["Capacity"]);
                    return ulong.MaxValue - sum < capacity ? ulong.MaxValue : sum + capacity;
                });
                var speeds = modules.Select(module => module["Speed"]?.ToString())
                    .Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToArray();
                var speedText = speeds.Length == 0 ? "speed unavailable" : $"reported speed: {string.Join(", ", speeds)} MHz";
                return $"Modules detected: {modules.Length}; module capacity total: {total / 1024d / 1024d / 1024d:0.0} GB; {speedText}.";
            }
            finally
            {
                foreach (var module in modules) module.Dispose();
            }
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException)
        {
            DiagnosticLogService.Write("Read memory module details", ex);
            return "Memory module details unavailable; total physical memory is reported separately.";
        }
    }

    private static string GetProcessorDetails()
    {
        var cores = GetWmiValue("SELECT NumberOfCores FROM Win32_Processor", "NumberOfCores");
        var threads = GetWmiValue("SELECT NumberOfLogicalProcessors FROM Win32_Processor", "NumberOfLogicalProcessors");
        var sockets = GetWmiCount("SELECT DeviceID FROM Win32_Processor");
        var maxClock = GetWmiValue("SELECT MaxClockSpeed FROM Win32_Processor", "MaxClockSpeed");
        var clockText = int.TryParse(maxClock, out var mhz) && mhz > 0 ? $"{mhz} MHz reported maximum" : "maximum clock unavailable";
        return $"Sockets: {sockets}; cores: {cores}; logical processors: {threads}; {clockText}.";
    }

    private static List<GraphicsAdapterInfo> CollectGraphicsAdapters()
    {
        var adapters = new List<GraphicsAdapterInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DriverVersion, DriverDate, VideoProcessor, AdapterRAM, CurrentHorizontalResolution, CurrentVerticalResolution, PNPDeviceID FROM Win32_VideoController");
            using var results = searcher.Get();

            foreach (ManagementObject item in results)
            {
                try
                {
                    var driverDate = FormatWmiDate(item["DriverDate"]?.ToString());
                    var memory = FormatAdapterMemory(item["AdapterRAM"]);
                    var width = item["CurrentHorizontalResolution"]?.ToString();
                    var height = item["CurrentVerticalResolution"]?.ToString();
                    var resolution = int.TryParse(width, out var w) && int.TryParse(height, out var h)
                        ? $"{w} x {h}"
                        : "";
                    var name = item["Name"]?.ToString() ?? "";
                    var pnpId = item["PNPDeviceID"]?.ToString() ?? "";

                    adapters.Add(new GraphicsAdapterInfo(
                        name,
                        item["DriverVersion"]?.ToString() ?? "",
                        driverDate,
                        item["VideoProcessor"]?.ToString() ?? "",
                        memory,
                        resolution,
                        ClassifyAdapter(name, pnpId)));
                }
                finally
                {
                    item.Dispose();
                }
            }
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            DiagnosticLogService.Write("Enumerate graphics adapters", ex);
        }

        return adapters;
    }

    private static string ClassifyAdapter(string name, string pnpId)
    {
        var vendor = pnpId.Contains("VEN_8086", StringComparison.OrdinalIgnoreCase) ||
                     name.Contains("Intel", StringComparison.OrdinalIgnoreCase) ? "Intel" :
                     pnpId.Contains("VEN_10DE", StringComparison.OrdinalIgnoreCase) ||
                     name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ? "NVIDIA" :
                     pnpId.Contains("VEN_1002", StringComparison.OrdinalIgnoreCase) ||
                     name.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
                     name.Contains("Radeon", StringComparison.OrdinalIgnoreCase) ? "AMD" : "";

        return string.IsNullOrEmpty(vendor)
            ? "Vendor not identified; GPU role not verified"
            : $"{vendor} GPU; integrated/discrete role not verified";
    }

    private static string FormatWmiDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        try { return ManagementDateTimeConverter.ToDateTime(value).ToString("yyyy-MM-dd"); }
        catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException)
        {
            DiagnosticLogService.Write("Parse graphics driver date", ex);
            return "";
        }
    }

    private static string FormatAdapterMemory(object? value)
    {
        var bytes = TryUInt64(value);
        return bytes == 0 ? "" : $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
    }

    private static ulong TryUInt64(object? value)
    {
        try { return value is null ? 0 : Convert.ToUInt64(value); }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private static string GetDriveSummary()
    {
        var entries = new List<string>();
        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;
                    entries.Add($"{drive.Name.TrimEnd('\\')} — {drive.TotalSize / 1024d / 1024d / 1024d:0} GB total, {drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0} GB free");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
                {
                    DiagnosticLogService.Write("Read fixed drive capacity", ex);
                    entries.Add($"{drive.Name} — capacity unavailable");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            DiagnosticLogService.Write("Enumerate fixed drives", ex);
            return "Storage inventory unavailable";
        }

        return entries.Count == 0 ? "No ready fixed drives found" : string.Join(Environment.NewLine, entries);
    }

    private static int GetWmiCount(string query)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();
            return results.Count;
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException)
        {
            DiagnosticLogService.Write("Count processor sockets", ex);
            return 0;
        }
    }

    private static string GetWmiValue(string query, string property)
    {
        var value = GetWmiRawValue(query, property);
        return string.IsNullOrWhiteSpace(value) ? "Unavailable" : value;
    }

    private static string GetWmiRawValue(string query, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();
            foreach (ManagementObject item in results)
            {
                using (item)
                    return item[property]?.ToString() ?? "";
            }
            return "";
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            DiagnosticLogService.Write("Read Windows hardware inventory", ex);
            return "";
        }
    }

    private static string GetRegistryValue(RegistryKey root, string path, string name)
    {
        try
        {
            using var key = root.OpenSubKey(path);
            return key?.GetValue(name)?.ToString() ?? "Unavailable";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            DiagnosticLogService.Write("Read DirectX registry value", ex);
            return "Unavailable";
        }
    }
}
