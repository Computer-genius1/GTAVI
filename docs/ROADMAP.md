# Roadmap

This roadmap tracks the independent Windows utility. Game-specific integration is not planned unless authorized PC components and required permissions become available.

## Phase 0 — Repository foundation
- [x] Create project README and clarify independent-project status.
- [x] Add architecture and roadmap documentation.
- [x] Add contribution, security, editor, and ignore conventions.
- [x] Choose C#/.NET 8 and WPF.
- [x] Add the initial Windows application shell.

## Phase 1 — Windows 11 foundation
- [x] Establish reproducible solution and Windows CI build.
- [x] Add Windows 11 build detection and platform gating.
- [x] Add x64 and experimental ARM64 classification.
- [x] Add read-only system diagnostics.
- [x] Add platform compatibility tests.
- [x] Add user-initiated JSON report export.

## Phase 2 — Diagnostics improvements
- [x] Report GPU names, drivers, reported adapter memory, and display modes.
- [x] Enumerate multiple WMI-reported graphics adapters and identify common vendors where possible.
- [x] Avoid guessing integrated-versus-discrete GPU roles from incomplete WMI data.
- [x] Test graphics formatting and multiple-adapter summaries.
- [x] Add CPU socket/core/thread and reported maximum-clock details.
- [x] Add memory-module count, capacity, and speed where Windows reports them.
- [x] Report total/free capacity for each ready fixed drive.
- [x] Add Direct3D 11 feature-level and Direct3D 12 device-creation checks.
- [x] Add graphics device health reporting and Windows error explanations.
- [x] Add overall Healthy, Needs Attention, and Unable to Verify summary.
- [x] Add diagnostic aggregation tests.
- [x] Isolate WMI and registry reads so missing component data does not discard all inventory.
- [x] Add bounded, privacy-conscious local error logging.
- [x] Add hardware-identifier redaction to JSON reports (enabled by default).
- [x] Add tests for report serialization and redaction.
- [ ] Add deeper tests for Windows-specific WMI failures and real hardware edge cases.
- [ ] Improve accessibility, interface scaling, and keyboard navigation.

## Phase 3 — Beta packaging and release engineering
- [x] Define preview version metadata (0.1.0-preview.1).
- [x] Add self-contained Windows x64 publishing to CI.
- [x] Package the published application as a portable ZIP.
- [x] Upload preview ZIP after successful build and test steps.
- [ ] Confirm download, extraction, and launch on a clean Windows 11 x64 machine.
- [ ] Validate multi-GPU enumeration on physical hybrid-graphics systems.
- [ ] Verify redacted and unredacted reports on real systems.
- [ ] Consider signed installer/MSIX after portable preview validation.
- [ ] Define stable release notes, versioning, and update/rollback behavior.

## Phase 4 — Authorized integration research
- [ ] Review official Rockstar announcements and distribution requirements.
- [ ] Confirm legal and technical feasibility before considering integration.
- [ ] Document verified requirements only from an authorized PC release.
- [ ] Prototype only against authorized components.

## First public beta criteria

The first public beta target is the Windows readiness utility, not a playable GTA VI PC port. Do not call it beta until:
- CI builds and tests pass on the current default branch.
- A downloadable Windows x64 package is generated from that successful run.
- The package launches on a clean Windows 11 x64 system.
- A complete scan and JSON export work without crashing.
- Redacted and unredacted report output has been reviewed.
- Known limitations, privacy behavior, and unsupported configurations are documented.
- At least one real-world test covers a system with more than one graphics adapter.

## Release policy

No release will be described as a playable GTA VI PC port unless it genuinely provides that functionality through authorized game components. Until then, releases will be clearly labeled as independent Windows diagnostic tools or previews.
