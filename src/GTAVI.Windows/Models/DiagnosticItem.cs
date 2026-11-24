namespace GTAVI.Windows.Models;

public sealed class DiagnosticItem
{
    public DiagnosticItem(string name, string value, string detail = "")
    {
        Name = name;
        Value = value;
        Detail = detail;
    }

    public string Name { get; }
    public string Value { get; }
    public string Detail { get; }
}
