using GTAVI.Windows.Models;
using GTAVI.Windows.Services;
using Xunit;

namespace GTAVI.Tests;

public sealed class GraphicsAdapterFormatterTests
{
    [Fact]
    public void FormatsAllAvailableAdapterDetails()
    {
        var adapters = new[]
        {
            new GraphicsAdapterInfo(
                "Example Graphics Adapter",
                "31.0.15.1234",
                "2026-10-15",
                "Example GPU",
                "8.0 GB",
                "2560 x 1440")
        };

        var report = GraphicsAdapterFormatter.Format(adapters);

        Assert.Contains("Example Graphics Adapter", report);
        Assert.Contains("31.0.15.1234", report);
        Assert.Contains("2026-10-15", report);
        Assert.Contains("Example GPU", report);
        Assert.Contains("8.0 GB", report);
        Assert.Contains("2560 x 1440", report);
    }

    [Fact]
    public void HandlesMissingAdapterFields()
    {
        var adapters = new[]
        {
            new GraphicsAdapterInfo("Example GPU", "", "", "", "", "")
        };

        var report = GraphicsAdapterFormatter.Format(adapters);

        Assert.Contains("Driver: Unavailable", report);
        Assert.Contains("Video processor: Unavailable", report);
        Assert.Contains("Adapter memory reported by Windows: Unavailable", report);
        Assert.Contains("Current display mode: Unavailable", report);
    }

    [Fact]
    public void HandlesEmptyAdapterCollection()
    {
        var report = GraphicsAdapterFormatter.Format(Array.Empty<GraphicsAdapterInfo>());

        Assert.Equal("No graphics adapters reported by Windows.", report);
    }
}
