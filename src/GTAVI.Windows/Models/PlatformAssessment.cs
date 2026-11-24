namespace GTAVI.Windows.Models;

public enum PlatformSupportLevel
{
    Unsupported,
    Supported,
    Experimental
}

public sealed record PlatformAssessment(
    PlatformSupportLevel Level,
    string Summary,
    string Details);
