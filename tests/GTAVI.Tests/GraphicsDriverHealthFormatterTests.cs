using GTAVI.Windows.Models;
using GTAVI.Windows.Services;
using Xunit;

namespace GTAVI.Tests;

public sealed class GraphicsDriverHealthFormatterTests
{
    [Fact]
    public void ReportsHealthyAdapterAndDriverDetails()
    {
        var report = new GraphicsDriverHealthReport(
            [new GraphicsDriverHealthInfo("Example GPU", "1.2.3", "2026-01-02", "OK", 0)],
            "");

        var formatted = GraphicsDriverHealthFormatter.Format(report);

        Assert.Contains("Example GPU", formatted);
        Assert.Contains("Device status: OK (Code 0)", formatted);
        Assert.Contains("Driver version: 1.2.3", formatted);
        Assert.Contains("Driver date: 2026-01-02", formatted);
    }

    [Theory]
    [InlineData(22, "The device is disabled.")]
    [InlineData(28, "The device drivers are not installed.")]
    [InlineData(43, "Windows stopped the device because it reported problems.")]
    public void ExplainsCommonWindowsDeviceErrors(int code, string expected)
    {
        var report = new GraphicsDriverHealthReport(
            [new GraphicsDriverHealthInfo("Example GPU", "", "", "Error", code)],
            "");

        var formatted = GraphicsDriverHealthFormatter.Format(report);

        Assert.Contains($"Code {code}", formatted);
        Assert.Contains(expected, formatted);
        Assert.Contains("Driver version: Not reported by Windows", formatted);
        Assert.Contains("Driver date: Not reported by Windows", formatted);
    }

    [Fact]
    public void DoesNotAssumeHealthyWhenWindowsDoesNotProvideErrorCode()
    {
        var report = new GraphicsDriverHealthReport(
            [new GraphicsDriverHealthInfo("Example GPU", "", "", "OK", null)],
            "");

        var formatted = GraphicsDriverHealthFormatter.Format(report);

        Assert.Contains("Unverified", formatted);
        Assert.Contains("no configuration code was provided", formatted);
    }

    [Fact]
    public void HandlesUnavailableInventoryAndUnknownErrorCodes()
    {
        var unavailable = GraphicsDriverHealthFormatter.Format(
            new GraphicsDriverHealthReport([], "Access denied while reading graphics device status."));
        var unknown = GraphicsDriverHealthFormatter.Format(
            new GraphicsDriverHealthReport(
                [new GraphicsDriverHealthInfo("Example GPU", "", "", "", 999)],
                ""));

        Assert.Equal("Access denied while reading graphics device status.", unavailable);
        Assert.Contains("Code 999", unknown);
        Assert.Contains("consult Device Manager", unknown);
    }
}
