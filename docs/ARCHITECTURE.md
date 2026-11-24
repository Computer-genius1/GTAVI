# Architecture

## Current implementation

The repository contains a Windows Presentation Foundation (WPF) desktop application written in C# and targeting .NET 8 for Windows.

The application is divided into:
- **UI:** WPF dashboard and user-initiated scan/export actions.
- **Models:** Small immutable diagnostic and platform-assessment data models.
- **Services:** Read-only Windows and hardware inventory, plus platform support assessment.
- **Tests:** Unit tests for Windows build and CPU architecture policy.
- **CI:** A Windows GitHub Actions workflow that restores, builds, and runs tests.

The current app is a system-readiness utility. It does not launch, modify, emulate, or provide access to GTA VI.

## Design principles

- Target Windows 11 build 22000 or later; do not add Windows 10 compatibility paths.
- x64 is the primary development architecture. ARM64 is explicitly experimental.
- Keep the Windows-side application separate from proprietary game code and assets.
- Use documented Windows APIs and lawfully obtained dependencies.
- Do not circumvent DRM, anti-cheat, platform security, or other access controls.
- Keep diagnostics transparent and privacy-conscious; collect no telemetry by default.
- Clearly distinguish verified platform facts from unknown game requirements.

## Implemented components

### 1. Windows application

The WPF application currently provides:
- A dark, scrollable diagnostics dashboard.
- A Windows 11 platform gate and architecture status.
- Multi-adapter graphics inventory with driver and current display-mode details.
- A user-initiated hardware and operating-system scan.
- A user-initiated JSON report export.

### 2. System inventory

The inventory service reads:
- Windows product name, display version, and build.
- Operating-system and process architecture.
- CPU name, core count, and logical processor count.
- Installed physical memory.
- Graphics adapter names, driver version and date, video processor, Windows-reported adapter memory, and current display resolution where WMI provides them.
- Windows Plug and Play `Status` and `ConfigManagerErrorCode` values for each display adapter, with common device errors translated into readable explanations.
- System manufacturer and model.
- BIOS version.
- DirectX registry version (informational only; it does not establish supported Direct3D feature levels).
- Ready local fixed-drive capacity and free space.

WMI and registry failures are handled as unavailable values where possible. The scan does not require administrator privileges by design, and results may be incomplete on systems where Windows restricts inventory access. GPU memory values are sourced from the WMI AdapterRAM field and may be capped or reported imprecisely by some drivers; they must not be treated as authoritative dedicated-VRAM measurements.

### 3. Graphics API capability probes

The scan attempts to create a Direct3D 11 hardware device and records the highest feature level returned by the runtime. It separately calls `D3D12CreateDevice` to check whether a Direct3D 12 device can be created at feature level 11_0 or higher. These probes use the default hardware adapter selected by Windows; they do not enumerate every adapter, test optional DirectX 12 features, benchmark performance, or establish compatibility with any game. Remote sessions, virtual adapters, driver state, or system policy can affect the result. The DirectX registry version remains informational and is not used as a substitute for these API checks.

### 4. Graphics driver health

The health check reads Windows-reported Plug and Play state from `Win32_VideoController`. It distinguishes a reported healthy configuration code, a known device error code, and an unknown/unavailable code. Common Windows configuration errors are explained in the report; unrecognized codes are preserved for troubleshooting in Device Manager. Driver version and date are displayed when Windows reports them, but missing metadata is not treated as proof of a broken or outdated driver. The utility does not compare versions against vendor releases, install drivers, or attempt repairs. WMI access failures are surfaced as a collection-status note.

### 5. Platform assessment

The current policy is intentionally narrow:
- Windows builds below 22000 are unsupported.
- Windows 11 x64 is the primary supported development target.
- Windows 11 ARM64 is marked experimental.
- Other architectures are unsupported.

This is an application platform policy, not a statement about GTA VI compatibility or requirements.

### 6. Automated tests

The test project covers the Windows build threshold and architecture classification. More tests are needed for WMI failures, report serialization, and UI behavior.

## Privacy and diagnostics

- Information is collected locally after the user selects Scan system.
- Reports are written only when the user selects Export report and chooses a destination.
- No telemetry or automatic upload is implemented.
- Reports should not include personal files, account credentials, or unrelated system data.

## Future authorized game integration

This layer is deliberately unimplemented. It can be investigated only after lawful access to an official PC build and any required authorization. It must not include copied game assets, proprietary code, DRM circumvention, or unofficial executable modifications.
