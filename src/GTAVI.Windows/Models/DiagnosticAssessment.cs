namespace GTAVI.Windows.Models;

public enum DiagnosticHealthStatus
{
    Healthy,
    NeedsAttention,
    UnableToVerify
}

public sealed record DiagnosticAssessment(
    DiagnosticHealthStatus Status,
    int HealthyCount,
    int NeedsAttentionCount,
    int UnableToVerifyCount,
    string Summary,
    string Details);
