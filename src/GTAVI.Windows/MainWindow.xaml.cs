using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using GTAVI.Windows.Models;
using GTAVI.Windows.Services;

namespace GTAVI.Windows;

public partial class MainWindow : Window
{
    private readonly SystemInfoService _systemInfoService = new();
    private readonly PlatformCompatibilityService _platformService = new();
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

            _diagnostics.Clear();
            foreach (var item in results)
                _diagnostics.Add(item);

            StatusText.Text = $"Scan complete • {results.Count} diagnostic categories • {DateTimeOffset.Now:t}";
            ExportButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"The scan could not be completed: {ex.Message}";
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
            var report = new
            {
                application = "GTAVI Windows Readiness",
                reportGenerated = DateTimeOffset.Now,
                collectionScope = "Local Windows system information only",
                diagnostics = _diagnostics
            };

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(dialog.FileName, json);
            StatusText.Text = $"Report saved: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"The report could not be saved.\n\n{ex.Message}",
                "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
