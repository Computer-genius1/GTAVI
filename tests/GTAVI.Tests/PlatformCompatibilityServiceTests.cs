using System.Runtime.InteropServices;
using GTAVI.Windows.Models;
using GTAVI.Windows.Services;

namespace GTAVI.Tests;

public sealed class PlatformCompatibilityServiceTests
{
    private readonly PlatformCompatibilityService _service = new();

    [Theory]
    [InlineData(19045)]
    [InlineData(21999)]
    public void OlderWindowsBuildsAreUnsupported(int build)
    {
        var result = _service.Assess(build, Architecture.X64);

        Assert.Equal(PlatformSupportLevel.Unsupported, result.Level);
        Assert.Contains("Windows 11 required", result.Summary);
    }

    [Theory]
    [InlineData(22000)]
    [InlineData(26100)]
    public void Windows11X64IsSupported(int build)
    {
        var result = _service.Assess(build, Architecture.X64);

        Assert.Equal(PlatformSupportLevel.Supported, result.Level);
        Assert.Equal("Windows 11 x64", result.Summary);
    }

    [Fact]
    public void Windows11Arm64IsExperimental()
    {
        var result = _service.Assess(26100, Architecture.Arm64);

        Assert.Equal(PlatformSupportLevel.Experimental, result.Level);
        Assert.Contains("experimental", result.Details);
    }

    [Fact]
    public void UnsupportedArchitectureIsRejected()
    {
        var result = _service.Assess(26100, Architecture.X86);

        Assert.Equal(PlatformSupportLevel.Unsupported, result.Level);
    }
}
