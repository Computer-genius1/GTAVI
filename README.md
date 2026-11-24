# GTA VI for Windows 11

An independent community software project building Windows 11 system-readiness and diagnostic tools related to the subject of Grand Theft Auto VI (GTA VI).

> [!IMPORTANT]
> This repository is **not an official Rockstar Games project**, is not endorsed by Rockstar Games, and is not affiliated with Take-Two Interactive. It does not contain GTA VI game assets, executable files, source code, or a playable PC port. This project does not claim to make the game run on PC, and it will not bypass platform protections or distribute copyrighted game content.

## Current status

**Status: Early development — Windows diagnostics dashboard, GPU inventory, Direct3D capability checks, graphics device health reporting, and an overall diagnostic status summary implemented.**

The native Windows desktop application can inspect local Windows and hardware information, report graphics adapter and driver details, and export a JSON report.

This is a system information utility, **not a GTA VI compatibility verdict**. No official PC system requirements are assumed or invented. There is no playable game, launcher, game compatibility layer, or performance patch.

## What is implemented

- Native Windows Presentation Foundation (WPF) desktop application using C# and .NET 8.
- Dark diagnostics dashboard.
- Windows edition, display version, and build detection.
- Windows 11 build gate (build 22000 or later).
- OS architecture detection: x64 supported for primary development; ARM64 marked experimental.
- CPU model, core/thread count, and installed physical memory reporting.
- Graphics adapter inventory, including driver version/date, video processor, Windows-reported adapter memory, and current display resolution where available.
- Direct3D 11 hardware-device detection with the highest reported feature level, plus a Direct3D 12 hardware-device creation check.
- Graphics device health reporting using Windows Plug and Play status and configuration error codes, with explanations for common errors and explicit handling of missing driver metadata.
- An overall diagnostic summary with **Healthy**, **Needs Attention**, and **Unable to Verify** outcomes based on the platform assessment, graphics driver state, and Direct3D device checks.
- System model, manufacturer, BIOS version, DirectX registry version, and fixed-drive capacity reporting.
- User-initiated JSON report export.
- Automated tests for platform compatibility, graphics adapter formatting, graphics API capability reporting, graphics driver health formatting, and overall diagnostic classification.
- GitHub Actions workflow to restore, build, and test on Windows.

All collected information is read locally. The app does not upload diagnostic data or collect personal files. GPU memory and driver details depend on what the installed Windows driver exposes through WMI and may be incomplete or rounded. Driver health reflects Windows-reported Plug and Play status; a missing driver version or date is reported as unavailable, not automatically labeled a fault. The app does not decide whether a driver is outdated or attempt to update or repair it. The overall status is a transparent summary of three Windows checks, not a numeric score or a GTA VI compatibility verdict. An experimental or incomplete result is labeled Unable to Verify rather than assumed healthy. Direct3D checks create devices on the default hardware adapter only; they do not enumerate every adapter, measure performance, or establish game compatibility.

## Platform direction

- **Operating system:** Windows 11 only (build 22000 or later).
- **Primary architecture:** x64 (AMD64).
- **Additional architecture:** ARM64 is experimental and requires further validation.
- **Development approach:** Windows-native components, separate from proprietary game code and assets.
- **Distribution:** No game binaries or Rockstar-owned assets are redistributed.

## Build and run

### Requirements

- Windows 11, build 22000 or later.
- .NET 8 SDK.
- Visual Studio 2022 with the **.NET desktop development** workload, or the .NET SDK and command line.

### Commands

From the repository root:

```powershell
dotnet restore GTAVI.sln
dotnet build GTAVI.sln --configuration Release
dotnet test GTAVI.sln --configuration Release
dotnet run --project src/GTAVI.Windows/GTAVI.Windows.csproj
```

The first build restores the System.Management dependency used for hardware inventory. A successful GitHub Actions run will provide an additional clean-build check; until that workflow completes, the project should be treated as unverified on a clean machine.

## Repository layout

```text
GTAVI/
├── .github/
│   └── workflows/
│       └── windows-build.yml
├── docs/
│   ├── ARCHITECTURE.md
│   └── ROADMAP.md
├── src/
│   └── GTAVI.Windows/
│       ├── Models/
│       ├── Services/
│       ├── App.xaml
│       ├── MainWindow.xaml
│       └── GTAVI.Windows.csproj
├── tests/
│   └── GTAVI.Tests/
├── GTAVI.sln
├── CONTRIBUTING.md
├── SECURITY.md
└── README.md
```

## Planned next steps

1. Improve diagnostic error categorization and WMI fallback behavior.
2. Add tests for report generation and inventory edge cases.
3. Add structured local logging and report redaction controls.
4. Improve accessibility, interface scaling, and packaging.
5. Research any future game integration only if authorized PC game components and required permissions become available.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening an issue or pull request. For security-sensitive reports, follow [SECURITY.md](SECURITY.md).

## Legal and trademarks

Grand Theft Auto, GTA VI, and related names and marks are the property of their respective owners. They are used here only to identify the subject of this independent community project. This repository does not claim ownership of those marks or any Rockstar Games intellectual property.

## Disclaimer

This is an experimental, independent project. It does not provide a playable PC version, guarantee compatibility with any game, or promise a release date or performance level. Any future game integration must use lawfully obtained and authorized components.
