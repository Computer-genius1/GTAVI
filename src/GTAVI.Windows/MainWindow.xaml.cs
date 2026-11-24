using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using GTAVI.Windows.Models;
using GTAVI.Windows.Services;

namespace GTAVI.Windows;

public partial class MainWindow : Window
{
    private readonly SystemInfoService _systemInfoService = new();
    private readonly PlatformCompatibilityService _platformService = new();
    private readonly DiagnosticAssessmentService _diagnosticAssessmentService = new();
    private readonly ObservableCollection<DiagnosticItem> _diagnostics = new();

    public MainWindow()
    {
        InitializeComponent();

        var platform = _platformService.AssessCurrentSystem();
        if (platform.Level == PlatformSupportLevel.Unsupported)
        {
            MessageBox.Show(
                $"{platform.Summary}\n\n{platform.Details}",
                "Unsupported platform",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Application.Current.Shutdown();
            return;
        }

        DiagnosticsList.ItemsSource = _diagnostics;
        StatusText.Text = platform.Level == PlatformSupportLevel.Experimental
            ? $"Ready • {platform.Summary} (experimental)"
            : $"Ready • {platform.Summary}";
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        ScanButton.IsEnabled = false;
        ExportButton.IsEnabled = false;
        StatusText.Text = "Scanning local system information…";

        try
        {
            var results = (await _systemInfoService.CollectAsync()).ToList();
            var platform = _platformService.AssessCurrentSystem();
            results.Insert(0, new DiagnosticItem(
                "Platform assessment",
                platform.Summary,
                platform.Details));

            var assessment = _diagnosticAssessmentService.Evaluate(results);
            results.Insert(0, new DiagnosticItem(
                $"Overall diagnostic status: {assessment.Status}",
                assessment.Summary,
                assessment.Details));

            _diagnostics.Clear();
            foreach (var item in results)
                _diagnostics.Add(item);

            StatusText.Text = $"Scan complete • {assessment.Status} • {DateTimeOffset.Now:t}";
            ExportButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            DiagnosticLogService.Write("Complete system scan", ex);
            StatusText.Text = "The scan could not be completed. See the local diagnostic log for details.";
        }
        finally
        {
            ScanButton.IsEnabled = true;
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_diagnostics.Count == 0)
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Export system diagnostics",
            FileName = $"GTAVI-System-Report-{DateTime.Now:yyyyMMdd-HHmmss}.json",
            DefaultExt = ".json",
            Filter = "JSON report (*.json)|*.json"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        try
        {
            var redactIdentifiers = RedactReportCheckBox.IsChecked == true;
            var json = ReportExportService.Serialize(_diagnostics, redactIdentifiers);
            await File.WriteAllTextAsync(dialog.FileName, json);
            StatusText.Text = redactIdentifiers
                ? $"Redacted report saved: {Path.GetFileName(dialog.FileName)}"
                : $"Report saved: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            DiagnosticLogService.Write("Export system report", ex);
            MessageBox.Show(this,
                $"The report could not be saved. A privacy-conscious error entry was written to the local diagnostic log.",
                "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
