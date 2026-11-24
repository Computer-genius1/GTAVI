using System.Text.Json;
using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

/// <summary>
/// Creates a JSON diagnostics report. Optional redaction removes identifying
/// hardware model strings while preserving the rest of the diagnostic results.
/// </summary>
public static class ReportExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly HashSet<string> SensitiveHardwareFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Processor",
        "Graphics adapters",
        "System manufacturer",
        "System model",
        "BIOS version"
    };

    public static string Serialize(
        IEnumerable<DiagnosticItem> diagnostics,
        bool redactHardwareIdentifiers,
        DateTimeOffset? generatedAt = null)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var items = diagnostics.Select(item =>
        {
            if (!redactHardwareIdentifiers || !SensitiveHardwareFields.Contains(item.Name))
                return item;

            return new DiagnosticItem(
                item.Name,
                "Redacted by user",
                "Hardware identifying details were omitted from this exported report.");
        }).ToArray();

        var report = new
        {
            application = "GTAVI Windows Readiness",
            reportGenerated = generatedAt ?? DateTimeOffset.Now,
            collectionScope = "Local Windows system information only",
            privacy = new
            {
                hardwareIdentifiersRedacted = redactHardwareIdentifiers
            },
            diagnostics = items
        };

        return JsonSerializer.Serialize(report, JsonOptions);
    }
}
