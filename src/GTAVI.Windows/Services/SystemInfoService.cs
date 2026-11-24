using System.Management;
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

            var items = new List<DiagnosticItem>
            {
                new("Operating system", GetWindowsName(), "Detected from the Windows registry."),
                new("System architecture", RuntimeInformation.OSArchitecture.ToString(),
                    $"Process architecture: {RuntimeInformation.ProcessArchitecture}"),
                new("Processor", GetWmiValue("SELECT Name FROM Win32_Processor", "Name"),
                    GetProcessorDetails()),
                new("Installed memory", GetMemory(), "Physical memory reported by Windows."),
                new("Graphics adapters", GetGraphicsAdapters(),
                    "Driver and display details are reported by Windows WMI. Adapter memory can be limited or rounded by the driver."),
                new("DirectX", GetRegistryValue(Registry.LocalMachine,
                    @"SOFTWARE\Microsoft\DirectX", "Version"), "Version reported by the DirectX registry key; this is not a feature-level test."),
                new("System manufacturer", GetWmiValue("SELECT Manufacturer FROM Win32_ComputerSystem", "Manufacturer")),
                new("System model", GetWmiValue("SELECT Model FROM Win32_ComputerSystem", "Model")),
                new("BIOS version", GetWmiValue("SELECT SMBIOSBIOSVersion FROM Win32_BIOS", "SMBIOSBIOSVersion")),
                new("Available drives", GetDriveSummary(), "Local fixed disks; free space is informational.")
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
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
        {
            return "Access denied";
        }
    }

    private static string GetMemory()
    {
        try
        {
            var bytes = Convert.ToUInt64(GetWmiRawValue(
                "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory"));
            return $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
        }
        catch
        {
            return "Unavailable";
        }
    }

    private static string GetProcessorDetails()
    {
        var cores = GetWmiValue("SELECT NumberOfCores FROM Win32_Processor", "NumberOfCores");
        var threads = GetWmiValue("SELECT NumberOfLogicalProcessors FROM Win32_Processor", "NumberOfLogicalProcessors");
        return $"Cores: {cores}; logical processors: {threads}";
    }

    private static string GetGraphicsAdapters()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DriverVersion, DriverDate, VideoProcessor, AdapterRAM, CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
            using var results = searcher.Get();

            var adapters = new List<GraphicsAdapterInfo>();
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

                    adapters.Add(new GraphicsAdapterInfo(
                        item["Name"]?.ToString() ?? "",
                        item["DriverVersion"]?.ToString() ?? "",
                        driverDate,
                        item["VideoProcessor"]?.ToString() ?? "",
                        memory,
                        resolution));
                }
                finally
                {
                    item.Dispose();
                }
            }

            return GraphicsAdapterFormatter.Format(adapters);
        }
        catch
        {
            return "Graphics adapter details are unavailable through Windows WMI.";
        }
    }

    private static string FormatWmiDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        try
        {
            return ManagementDateTimeConverter.ToDateTime(value).ToString("yyyy-MM-dd");
        }
        catch
        {
            return "";
        }
    }

    private static string FormatAdapterMemory(object? value)
    {
        try
        {
            if (value is null)
                return "";

            var bytes = Convert.ToUInt64(value);
            if (bytes == 0)
                return "";

            return $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
        }
        catch
        {
            return "";
        }
    }

    private static string GetDriveSummary()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Fixed)
                .Select(drive => $"{drive.Name.TrimEnd('\\')} — {drive.TotalSize / 1024d / 1024d / 1024d:0} GB total, {drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0} GB free")
                .ToArray();
            return drives.Length == 0 ? "No ready fixed drives found" : string.Join(Environment.NewLine, drives);
        }
        catch
        {
            return "Unavailable";
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
            var first = results.Cast<ManagementObject>().FirstOrDefault();
            return first?[property]?.ToString() ?? "";
        }
        catch
        {
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
        catch
        {
            return "Unavailable";
        }
    }
}
