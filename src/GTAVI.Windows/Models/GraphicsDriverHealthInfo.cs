namespace GTAVI.Windows.Models;

public sealed record GraphicsDriverHealthInfo(
    string AdapterName,
    string DriverVersion,
    string DriverDate,
    string WindowsStatus,
    int? ConfigurationManagerErrorCode);

public sealed record GraphicsDriverHealthReport(
    IReadOnlyList<GraphicsDriverHealthInfo> Adapters,
    string CollectionStatus);
