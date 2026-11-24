using System.Runtime.InteropServices;
using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

/// <summary>
/// Probes Direct3D device creation on the default hardware adapter.
/// This is a capability check, not a game compatibility or performance test.
/// </summary>
public sealed class GraphicsApiCapabilityService
{
    private const uint D3D11SdkVersion = 7;
    private const uint D3DDriverTypeHardware = 1;
    private const uint FeatureLevel12_2 = 0xC200;
    private const uint FeatureLevel12_1 = 0xC100;
    private const uint FeatureLevel12_0 = 0xC000;
    private const uint FeatureLevel11_1 = 0xB100;
    private const uint FeatureLevel11_0 = 0xB000;
    private const uint FeatureLevel10_1 = 0xA100;
    private const uint FeatureLevel10_0 = 0xA000;
    private const uint FeatureLevel9_3 = 0x9300;

    private static readonly uint[] SupportedFeatureLevels =
    [
        FeatureLevel12_2,
        FeatureLevel12_1,
        FeatureLevel12_0,
        FeatureLevel11_1,
        FeatureLevel11_0,
        FeatureLevel10_1,
        FeatureLevel10_0,
        FeatureLevel9_3
    ];

    private static readonly Guid Direct3D12DeviceIid =
        new("189819f1-1db6-4b57-be54-1821339b85f7");

    public GraphicsApiCapabilities Detect()
    {
        if (!OperatingSystem.IsWindows())
        {
            return new GraphicsApiCapabilities(
                false, "", false,
                "Windows is required",
                "Windows is required");
        }

        var d3d11 = ProbeDirect3D11();
        var d3d12 = ProbeDirect3D12();

        return new GraphicsApiCapabilities(
            d3d11.Available,
            d3d11.FeatureLevel,
            d3d12.Available,
            d3d11.Status,
            d3d12.Status);
    }

    private static (bool Available, string FeatureLevel, string Status) ProbeDirect3D11()
    {
        IntPtr device = IntPtr.Zero;
        IntPtr context = IntPtr.Zero;

        try
        {
            var result = D3D11CreateDevice(
                IntPtr.Zero,
                D3DDriverTypeHardware,
                IntPtr.Zero,
                0,
                SupportedFeatureLevels,
                (uint)SupportedFeatureLevels.Length,
                D3D11SdkVersion,
                out device,
                out var featureLevel,
                out context);

            if (result < 0)
                return (false, "", $"Device creation failed (HRESULT 0x{result:X8})");

            return (true, FormatFeatureLevel(featureLevel), "Hardware device created");
        }
        catch (DllNotFoundException)
        {
            return (false, "", "Direct3D 11 runtime is unavailable");
        }
        catch (EntryPointNotFoundException)
        {
            return (false, "", "Direct3D 11 entry point is unavailable");
        }
        catch (Exception ex) when (ex is BadImageFormatException or MarshalDirectiveException)
        {
            return (false, "", $"Probe failed ({ex.GetType().Name})");
        }
        finally
        {
            ReleaseComObject(context);
            ReleaseComObject(device);
        }
    }

    private static (bool Available, string Status) ProbeDirect3D12()
    {
        IntPtr device = IntPtr.Zero;

        try
        {
            var result = D3D12CreateDevice(
                IntPtr.Zero,
                FeatureLevel11_0,
                ref Direct3D12DeviceIid,
                out device);

            return result >= 0 && device != IntPtr.Zero
                ? (true, "Hardware device created")
                : (false, $"Device creation failed (HRESULT 0x{result:X8})");
        }
        catch (DllNotFoundException)
        {
            return (false, "Direct3D 12 runtime is unavailable");
        }
        catch (EntryPointNotFoundException)
        {
            return (false, "Direct3D 12 entry point is unavailable");
        }
        catch (Exception ex) when (ex is BadImageFormatException or MarshalDirectiveException)
        {
            return (false, $"Probe failed ({ex.GetType().Name})");
        }
        finally
        {
            ReleaseComObject(device);
        }
    }

    private static string FormatFeatureLevel(uint featureLevel) => featureLevel switch
    {
        FeatureLevel12_2 => "12_2",
        FeatureLevel12_1 => "12_1",
        FeatureLevel12_0 => "12_0",
        FeatureLevel11_1 => "11_1",
        FeatureLevel11_0 => "11_0",
        FeatureLevel10_1 => "10_1",
        FeatureLevel10_0 => "10_0",
        FeatureLevel9_3 => "9_3",
        _ => $"0x{featureLevel:X4}"
    };

    private static void ReleaseComObject(IntPtr value)
    {
        if (value != IntPtr.Zero)
            Marshal.Release(value);
    }

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int D3D11CreateDevice(
        IntPtr adapter,
        uint driverType,
        IntPtr software,
        uint flags,
        [In] uint[] featureLevels,
        uint featureLevelsCount,
        uint sdkVersion,
        out IntPtr device,
        out uint featureLevel,
        out IntPtr immediateContext);

    [DllImport("d3d12.dll", ExactSpelling = true)]
    private static extern int D3D12CreateDevice(
        IntPtr adapter,
        uint minimumFeatureLevel,
        ref Guid riid,
        out IntPtr device);
}
