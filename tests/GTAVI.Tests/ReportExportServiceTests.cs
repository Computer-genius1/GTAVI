using System.Text.Json;
using GTAVI.Windows.Models;
using GTAVI.Windows.Services;
using Xunit;

namespace GTAVI.Tests;

public sealed class ReportExportServiceTests
{
    [Fact]
    public void SerializesDiagnosticReportWithPrivacyMetadata()
    {
        var items = new[]
        {
            new DiagnosticItem("Installed memory", "16.0 GB", "Physical memory"),
            new DiagnosticItem("Processor", "Example CPU", "8 cores")
        };
        var timestamp = new DateTimeOffset(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

        var json = ReportExportService.Serialize(items, false, timestamp);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("GTAVI Windows Readiness", root.GetProperty("application").GetString());
        Assert.False(root.GetProperty("privacy").GetProperty("hardwareIdentifiersRedacted").GetBoolean());
        Assert.Contains("Example CPU", json);
        Assert.Equal(timestamp, root.GetProperty("reportGenerated").GetDateTimeOffset());
    }

    [Fact]
    public void RedactsHardwareIdentifiersButPreservesDiagnosticLabels()
    {
        var items = new[]
        {
            new DiagnosticItem("Processor", "Example CPU", "8 cores; 16 threads"),
            new DiagnosticItem("Graphics adapters", "Example GPU", "Driver 1.2.3"),
            new DiagnosticItem("System model", "Example Laptop", ""),
            new DiagnosticItem("Installed memory", "16.0 GB", "Physical memory")
        };

        var json = ReportExportService.Serialize(items, true);
        using var document = JsonDocument.Parse(json);
        var diagnostics = document.RootElement.GetProperty("diagnostics");

        Assert.True(document.RootElement.GetProperty("privacy").GetProperty("hardwareIdentifiersRedacted").GetBoolean());
        Assert.Equal("Redacted by user", diagnostics[0].GetProperty("Value").GetString());
        Assert.Equal("Redacted by user", diagnostics[1].GetProperty("Value").GetString());
        Assert.Equal("Redacted by user", diagnostics[2].GetProperty("Value").GetString());
        Assert.Equal("16.0 GB", diagnostics[3].GetProperty("Value").GetString());
        Assert.DoesNotContain("Example CPU", json);
        Assert.DoesNotContain("Example GPU", json);
        Assert.DoesNotContain("Example Laptop", json);
    }

    [Fact]
    public void RejectsNullDiagnosticCollection()
    {
        Assert.Throws<ArgumentNullException>(() => ReportExportService.Serialize(null!, true));
    }
}
