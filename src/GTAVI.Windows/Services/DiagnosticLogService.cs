using System.IO;

namespace GTAVI.Windows.Services;

/// <summary>
/// Writes small local diagnostic logs. Log entries intentionally omit raw
/// hardware values, report contents, file paths, and exception messages.
/// </summary>
public static class DiagnosticLogService
{
    private static readonly object Sync = new();
    private const long MaximumLogBytes = 512 * 1024;

    public static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GTAVI", "Logs");

    public static void Write(string operation, Exception exception)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(LogDirectory);
                var path = Path.Combine(LogDirectory, "diagnostics.log");
                if (File.Exists(path) && new FileInfo(path).Length > MaximumLogBytes)
                {
                    var previous = path + ".old";
                    if (File.Exists(previous)) File.Delete(previous);
                    File.Move(path, previous);
                }

                var safeOperation = string.Concat((operation ?? "Unknown operation")
                    .Where(character => char.IsLetterOrDigit(character) || character is ' ' or '-' or '_'))
                    .Trim();
                var line = $"{DateTimeOffset.Now:O} | {safeOperation} | {exception.GetType().Name}{Environment.NewLine}";
                File.AppendAllText(path, line);
            }
        }
        catch
        {
            // Logging must never interrupt a diagnostic scan or report export.
        }
    }
}
