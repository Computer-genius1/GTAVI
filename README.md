# GTA VI for Windows 11

An independent community Windows 11 system-readiness and diagnostics project related to Grand Theft Auto VI.

> [!IMPORTANT]
> This is not an official Rockstar Games project and is not endorsed by Rockstar Games or Take-Two Interactive. It does not contain GTA VI assets, executable files, source code, or a playable PC port. It does not bypass platform protections or distribute copyrighted game content.

## Current status

**Early development — Windows diagnostics dashboard with multi-adapter GPU inventory, Direct3D capability checks, graphics driver health, overall diagnostic status, privacy-aware JSON export, and local error logging. CI creates a self-contained Windows x64 preview after successful build and test steps.**

This is a system information utility, **not a GTA VI compatibility verdict**. No official PC system requirements are assumed or invented. There is no playable game, launcher, compatibility layer, or performance patch.

## Implemented functionality

- Native WPF desktop application using C# and .NET 8.
- Windows edition, display version, build, and architecture detection; Windows 11 build 22000+ required, x64 primary, ARM64 experimental.
- CPU model, socket count, core count, logical processor count, and reported maximum clock.
- Installed physical memory and, where WMI provides it, memory-module count, aggregate capacity, and reported speed.
- Multiple graphics adapter inventory, vendor identification where possible, driver version/date, video processor, reported adapter memory, and current display resolution.
- Explicitly cautious GPU role reporting: WMI does not reliably prove whether an adapter is integrated or discrete, so the app does not guess.
- Direct3D 11 feature-level detection and Direct3D 12 device-creation check on the default Windows-selected hardware adapter.
- Graphics driver health using Windows Plug and Play status and configuration error codes.
- Overall diagnostic summary: Healthy, Needs Attention, or Unable to Verify.
- System model, manufacturer, BIOS version, DirectX registry version, and per-volume fixed-drive capacity/free-space reporting.
- User-initiated JSON report export with hardware-identifier redaction enabled by default. Redaction covers processor, GPU, manufacturer, system model, and BIOS identifying strings.
- Bounded local diagnostic logs under %LOCALAPPDATA%\GTAVI\Logs. Entries contain timestamps, sanitized operation labels, and exception types—not report contents, hardware values, file paths, or exception messages.
- Automated tests for platform compatibility, graphics adapter formatting and multi-adapter summaries, graphics API capability reporting, driver health, diagnostic classification, JSON serialization, and redaction.
- GitHub Actions restore, build, test, self-contained Windows x64 publish, ZIP packaging, and artifact upload.

## Diagnostic and privacy limitations

All inventory is read locally. The app does not upload diagnostic data or collect personal files. GPU memory and driver details depend on Windows WMI and may be incomplete or rounded. Vendor names are inferred from adapter names and Plug and Play vendor IDs. Integrated-versus-discrete roles are not inferred. Direct3D probes do not test every enumerated GPU. Driver health reflects Windows-reported Plug and Play status; the app does not determine whether a driver is outdated or attempt repairs.

The overall status summarizes selected Windows checks; it is not a benchmark or GTA VI compatibility verdict. A redacted report retains useful diagnostic categories and non-identifying values, so review the JSON before sharing it. Users can explicitly disable redaction before export.

## Build and run

Requirements: Windows 11 build 22000 or later, .NET 8 SDK, and Visual Studio 2022 with the .NET desktop development workload (or the .NET SDK).

From the repository root:

```powershell
dotnet restore GTAVI.sln
dotnet build GTAVI.sln --configuration Release
dotnet test GTAVI.sln --configuration Release
dotnet run --project src/GTAVI.Windows/GTAVI.Windows.csproj
```

## Downloading a CI preview

1. Open the repository's Actions tab.
2. Select a completed Windows build run with a successful conclusion.
3. Under Artifacts, download GTAVI-Windows-Readiness-win-x64.
4. Extract the ZIP and run GTAVI.Windows.exe on Windows 11 x64.

The preview is self-contained and includes the .NET runtime. It is a portable ZIP, not an installer. CI artifacts are temporary (14-day retention), unsigned, and development previews rather than stable releases. Do not use an artifact from a failed or untrusted workflow run.

## Planned next steps

1. Add deeper tests for Windows-specific WMI failures and hardware inventory edge cases.
2. Validate multi-GPU enumeration on physical hybrid-graphics systems.
3. Improve accessibility, interface scaling, and keyboard navigation.
4. Validate the preview on a clean Windows 11 x64 machine before calling it a public beta.
5. Consider a signed installer only after the portable preview is validated.
6. Research future game integration only if authorized PC game components and required permissions become available.

## Contributing

Read CONTRIBUTING.md before opening an issue or pull request. For security-sensitive reports, follow SECURITY.md.

## Legal and disclaimer

Grand Theft Auto, GTA VI, and related names and marks are the property of their respective owners and are used only to identify the subject of this independent project. This experimental utility does not provide a playable PC version, guarantee compatibility with any game, or promise a release date or performance level. Any future game integration must use lawfully obtained and authorized components.
