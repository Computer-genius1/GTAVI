namespace GTAVI.Windows.Models;

public sealed record GraphicsApiCapabilities(
    bool Direct3D11Available,
    string Direct3D11FeatureLevel,
    bool Direct3D12Available,
    string Direct3D11Status,
    string Direct3D12Status);
