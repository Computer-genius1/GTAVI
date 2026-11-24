using GTAVI.Windows.Models;

namespace GTAVI.Windows.Services;

public sealed class DiagnosticAssessmentService
{
    public DiagnosticAssessment Evaluate(IEnumerable<DiagnosticItem> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var items = diagnostics.ToList();
        var statuses = new List<DiagnosticHealthStatus>
        {
            AssessPlatform(Find(items, "Platform assessment")),
            AssessGraphicsDriverHealth(Find(items, "Graphics driver health")),
            AssessGraphicsApi(Find(items, "Graphics API capabilities"))
        };

        var healthy = statuses.Count(status => status == DiagnosticHealthStatus.Healthy);
        var needsAttention = statuses.Count(status => status == DiagnosticHealthStatus.NeedsAttention);
        var unableToVerify = statuses.Count(status => status == DiagnosticHealthStatus.UnableToVerify);

        var overall = needsAttention > 0
            ? DiagnosticHealthStatus.NeedsAttention
            : unableToVerify > 0
                ? DiagnosticHealthStatus.UnableToVerify
                : DiagnosticHealthStatus.Healthy;

        var summary = overall switch
        {
            DiagnosticHealthStatus.Healthy => "Healthy — no issues detected in the checks that could be verified.",
            DiagnosticHealthStatus.NeedsAttention => "Needs Attention — one or more checks reported a condition worth reviewing.",
            _ => "Unable to Verify — one or more checks did not provide enough information for a complete assessment."
        };

        var details = $"Checks: {healthy} healthy, {needsAttention} need attention, {unableToVerify} unable to verify. " +
                      "This is a summary of selected Windows diagnostics only; it is not a GTA VI compatibility result.";

        return new DiagnosticAssessment(overall, healthy, needsAttention, unableToVerify, summary, details);
    }

    private static DiagnosticHealthStatus AssessPlatform(DiagnosticItem? item)
    {
        if (item is null)
            return DiagnosticHealthStatus.UnableToVerify;

        var platformText = $"{item.Value} {item.Detail}";

        if (platformText.Contains("Unsupported", StringComparison.OrdinalIgnoreCase) ||
            platformText.Contains("required", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.NeedsAttention;

        if (platformText.Contains("experimental", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.UnableToVerify;

        if (item.Value.Contains("Windows 11 x64", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.Healthy;

        return DiagnosticHealthStatus.UnableToVerify;
    }

    private static DiagnosticHealthStatus AssessGraphicsDriverHealth(DiagnosticItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Value))
            return DiagnosticHealthStatus.UnableToVerify;

        if (item.Value.Contains("Attention required", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.NeedsAttention;

        if (item.Value.Contains("Unverified", StringComparison.OrdinalIgnoreCase) ||
            item.Value.Contains("Unknown", StringComparison.OrdinalIgnoreCase) ||
            item.Value.Contains("unavailable", StringComparison.OrdinalIgnoreCase) ||
            item.Value.Contains("No graphics adapters", StringComparison.OrdinalIgnoreCase) ||
            item.Value.Contains("not returned", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.UnableToVerify;

        if (item.Value.Contains("OK (Code 0)", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.Healthy;

        return DiagnosticHealthStatus.UnableToVerify;
    }

    private static DiagnosticHealthStatus AssessGraphicsApi(DiagnosticItem? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Value))
            return DiagnosticHealthStatus.UnableToVerify;

        var direct3D11Available = item.Value.Contains(
            "Direct3D 11: Available", StringComparison.OrdinalIgnoreCase);
        var direct3D12Available = item.Value.Contains(
            "Direct3D 12: Available", StringComparison.OrdinalIgnoreCase);

        if (direct3D11Available && direct3D12Available)
            return DiagnosticHealthStatus.Healthy;

        if (item.Value.Contains("Direct3D 11: Unavailable", StringComparison.OrdinalIgnoreCase) ||
            item.Value.Contains("Direct3D 12: Unavailable", StringComparison.OrdinalIgnoreCase))
            return DiagnosticHealthStatus.NeedsAttention;

        return DiagnosticHealthStatus.UnableToVerify;
    }

    private static DiagnosticItem? Find(IEnumerable<DiagnosticItem> items, string name) =>
        items.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
}
