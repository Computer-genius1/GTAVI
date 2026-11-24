using GTAVI.Windows.Models;
using GTAVI.Windows.Services;
using Xunit;

namespace GTAVI.Tests;

public sealed class GraphicsApiCapabilityFormatterTests
{
    [Fact]
    public void FormatsAvailableDirect3DApis()
    {
        var capabilities = new GraphicsApiCapabilities(
            true, "12_1", true, "Hardware device created", "Hardware device created");

        var report = GraphicsApiCapabilityFormatter.Format(capabilities);

        Assert.Contains("Direct3D 11: Available (Feature Level 12_1)", report);
        Assert.Contains("Direct3D 12: Available", report);
    }

    [Fact]
    public void ExplainsUnavailableApis()
    {
        var capabilities = new GraphicsApiCapabilities(
            false, "", false, "Device creation failed (HRESULT 0x80004005)", "Runtime unavailable");

        var report = GraphicsApiCapabilityFormatter.Format(capabilities);

        Assert.Contains("Direct3D 11: Unavailable (Device creation failed", report);
        Assert.Contains("Direct3D 12: Unavailable (Runtime unavailable)", report);
    }

    [Fact]
    public void HandlesMissingFeatureLevelAndStatusText()
    {
        var capabilities = new GraphicsApiCapabilities(false, "", false, "", "");

        var report = GraphicsApiCapabilityFormatter.Format(capabilities);

        Assert.Contains("No additional details", report);
    }
}
