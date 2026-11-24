namespace GTAVI.Windows.Models;

public sealed record GraphicsAdapterInfo(
    string Name,
    string DriverVersion,
    string DriverDate,
    string VideoProcessor,
    string AdapterMemory,
    string DisplayResolution);
