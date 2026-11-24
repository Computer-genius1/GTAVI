# Architecture

## Current implementation

The repository contains a Windows Presentation Foundation (WPF) desktop application written in C# and targeting .NET 8 for Windows.

The application is divided into:
- **UI:** WPF dashboard and user-initiated scan/export actions, including report redaction.
- **Models:** Diagnostic and platform-assessment data models.
- **Services:** Read-only Windows and hardware inventory, diagnostic assessment, report serialization/redaction, and privacy-conscious local logging.
- **Tests:** Unit tests for platform policy, graphics adapter formatting, driver health, diagnostic assessment, and JSON report serialization/redaction.
- **CI:** Windows workflow that restores, builds, tests, publishes a self-contained Windows x64 preview, and uploads the ZIP only after successful build and test steps.

The app is a system-readiness utility. It does not launch, modify, emulate, or provide access to GTA VI.

## Design principles

- Target Windows 11 build 22000 or later; x64 is primary and ARM64 experimental.
- Keep this application separate from proprietary game code and assets.
- Use documented Windows APIs and lawful dependencies.
- Do not circumvent DRM, anti-cheat, platform security, or access controls.
- Keep diagnostics local, transparent, and privacy-conscious.
- Distinguish verified Windows facts from unknown game requirements.

## System inventory

The service reads Windows edition/version/build, OS and process architecture, CPU name/socket/core/thread counts and reported maximum clock, installed physical memory and available memory-module details, all display adapters returned by Win32_VideoController, Windows Plug and Play device status, system manufacturer/model, BIOS version, DirectX registry version, and each ready fixed volume's capacity/free space.

Graphics adapter entries include names, driver version/date, video processor, WMI-reported adapter memory, display resolution, and vendor identification where possible. Vendor identification uses adapter names or Plug and Play vendor IDs. WMI does not reliably establish whether an adapter is integrated or discrete, so the app does not guess. It enumerates all adapters WMI reports, but Direct3D capability probes still use the default Windows-selected hardware adapter.

Collection is isolated by component: missing or inaccessible WMI/registry fields are reported as unavailable where possible, and one inaccessible drive does not discard other drive results. If memory-module details fail, total physical memory remains a separate field when available. The scan is designed not to require administrator privileges. WMI AdapterRAM can be capped or imprecise and is not authoritative dedicated-VRAM measurement.

## Graphics API capability probes

The scan attempts Direct3D 11 hardware-device creation and records the highest returned feature level. It separately calls D3D12CreateDevice to check whether a Direct3D 12 device can be created at feature level 11_0 or higher. These probes use the default hardware adapter selected by Windows; they do not test every adapter, optional DirectX 12 features, performance, or game compatibility. Remote sessions, virtual adapters, driver state, and system policy can affect results.

## Graphics driver health and overall assessment

The health check reads Windows Plug and Play state from Win32_VideoController. It distinguishes healthy configuration codes, known device errors, and unknown/unavailable codes. Missing driver version/date is not automatically treated as a fault. The utility does not compare versions against vendor releases, install drivers, or repair them.

The assessment service classifies platform policy, graphics driver health, and Direct3D device creation as Healthy, Needs Attention, or Unable to Verify. Needs Attention takes precedence; otherwise unverified checks prevent an overall Healthy result. This is not a weighted score, benchmark, or GTA VI compatibility verdict.

## Report privacy and local logging

The JSON report exporter supports hardware-identifier redaction. The UI enables it by default. When enabled, processor, graphics adapter, graphics-driver adapter names, system manufacturer, system model, and BIOS identifying strings are replaced with a redaction marker while other diagnostic values remain. Users can explicitly turn redaction off before exporting and should review reports before sharing them.

Local logs are written to %LOCALAPPDATA%\GTAVI\Logs\diagnostics.log. Entries contain timestamp, sanitized operation label, and exception type only. They omit raw hardware values, report contents, file paths, and exception messages. The log rotates at a bounded size. Logging failures do not interrupt scanning or exporting. Logs are not uploaded.

## Platform policy

- Windows builds below 22000 are unsupported.
- Windows 11 x64 is the primary supported development target.
- Windows 11 ARM64 is experimental.
- Other architectures are unsupported.

This is application platform policy, not a GTA VI requirement.

## Tests and preview packaging

Tests cover Windows build/architecture policy, graphics formatting including multi-adapter summaries, driver-health formatting, diagnostic classification, and report serialization/redaction. More tests are needed for Windows-specific WMI failure modes and UI behavior.

After restore, build, and tests succeed, CI publishes a self-contained win-x64 application, creates a portable ZIP, and uploads it as a GitHub Actions artifact with 14-day retention. This is a development preview, not an installer or signed stable release. It must be launched and exercised on a clean physical Windows 11 x64 machine before public beta.

## Future authorized game integration

This layer is deliberately unimplemented. It can be investigated only after lawful access to an official PC build and required authorization. It must not include copied game assets, proprietary code, DRM circumvention, or unofficial executable modifications.
