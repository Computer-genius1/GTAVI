using System.Runtime.InteropServices;
using Microsoft.Win32;
using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public sealed class PlatformCompatibilityService
{
    public const int MinimumWindows11Build = 22000;

    public PlatformAssessment AssessCurrentSystem()
    {
        var build = ReadWindowsBuild();
        return Assess(build, RuntimeInformation.OSArchitecture);
    }

    public PlatformAssessment Assess(int windowsBuild, Architecture architecture)
    {
        if (windowsBuild < MinimumWindows11Build)
        {
            return new PlatformAssessment(
                PlatformSupportLevel.Unsupported,
                "Windows 11 required",
                $"Detected Windows build {windowsBuild}; this application targets Windows 11 build {MinimumWindows11Build} or later.");
        }

        return architecture switch
        {
            Architecture.X64 => new PlatformAssessment(
                PlatformSupportLevel.Supported,
                "Windows 11 x64",
                $"Windows build {windowsBuild}. Primary development architecture."),
            Architecture.Arm64 => new PlatformAssessment(
                PlatformSupportLevel.Experimental,
                "Windows 11 ARM64",
                $"Windows build {windowsBuild}. Architecture detection is available; broader validation remains experimental."),
            _ => new PlatformAssessment(
                PlatformSupportLevel.Unsupported,
                "Unsupported architecture",
                $"Detected architecture: {architecture}. This build targets x64; ARM64 is experimental.")
        };
    }

    private static int ReadWindowsBuild()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return int.TryParse(key?.GetValue("CurrentBuildNumber")?.ToString(), out var build)
                ? build
                : 0;
        }
        catch
        {
            return 0;
        }
    }
}
