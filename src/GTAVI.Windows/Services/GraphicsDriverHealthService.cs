using System.Management;
using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

/// <summary>
/// Reads Windows Plug and Play status for display adapters. It does not judge
/// driver freshness or attempt to install, update, or repair drivers.
/// </summary>
public sealed class GraphicsDriverHealthService
{
    public GraphicsDriverHealthReport Collect()
    {
        if (!OperatingSystem.IsWindows())
            return new GraphicsDriverHealthReport([], "Windows is required for graphics device health checks.");

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DriverVersion, DriverDate, Status, ConfigManagerErrorCode FROM Win32_VideoController");
            using var results = searcher.Get();

            var adapters = new List<GraphicsDriverHealthInfo>();
            foreach (ManagementObject item in results)
            {
                try
                {
                    adapters.Add(new GraphicsDriverHealthInfo(
                        item["Name"]?.ToString() ?? "",
                        item["DriverVersion"]?.ToString() ?? "",
                        FormatDriverDate(item["DriverDate"]?.ToString()),
                        item["Status"]?.ToString() ?? "",
                        ToNullableInt(item["ConfigManagerErrorCode"])));
                }
                finally
                {
                    item.Dispose();
                }
            }

            return new GraphicsDriverHealthReport(
                adapters,
                adapters.Count == 0 ? "Windows did not return any graphics adapters." : "");
        }
        catch (UnauthorizedAccessException)
        {
            return new GraphicsDriverHealthReport([], "Access denied while reading graphics device status.");
        }
        catch (System.Security.SecurityException)
        {
            return new GraphicsDriverHealthReport([], "Windows security policy blocked graphics device inventory.");
        }
        catch (ManagementException ex)
        {
            return new GraphicsDriverHealthReport([], $"Windows Management Instrumentation could not read graphics device status ({ex.ErrorCode}).");
        }
        catch (Exception ex) when (ex is InvalidOperationException or COMException)
        {
            return new GraphicsDriverHealthReport([], $"Graphics device status is unavailable ({ex.GetType().Name}).");
        }
    }

    private static int? ToNullableInt(object? value)
    {
        if (value is null)
            return null;

        try
        {
            return Convert.ToInt32(value);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static string FormatDriverDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        try
        {
            return ManagementDateTimeConverter.ToDateTime(value).ToString("yyyy-MM-dd");
        }
        catch (FormatException)
        {
            return "";
        }
        catch (ArgumentOutOfRangeException)
        {
            return "";
        }
    }
}
