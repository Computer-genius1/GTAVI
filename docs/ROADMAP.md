# Roadmap

This roadmap tracks the independent Windows utility. Game-specific integration is not planned unless authorized PC components and required permissions become available.

## Phase 0 — Repository foundation
- [x] Create project README and clarify independent-project status.
- [x] Add architecture and roadmap documentation.
- [x] Add contribution, security, editor, and ignore conventions.
- [x] Choose C#/.NET 8 and WPF for the Windows desktop application.
- [x] Add the initial Windows application shell.

## Phase 1 — Windows 11 foundation
- [x] Establish a reproducible solution and Windows CI build.
- [x] Add Windows 11 build detection and platform gating.
- [x] Add x64 and experimental ARM64 classification.
- [x] Add read-only system diagnostics.
- [x] Add automated tests for platform compatibility rules.
- [x] Add user-initiated JSON report export.
- [x] Validate a clean build and test run through GitHub Actions.

## Phase 2 — Diagnostics improvements
- [x] Report GPU names, driver versions and dates, video processor, reported adapter memory, and current display resolution where available.
- [x] Add unit tests for graphics adapter report formatting.
- [x] Add Direct3D 11 feature-level detection and Direct3D 12 device-creation capability checks using documented Windows APIs.
- [ ] Add diagnostic error categorization and more resilient WMI fallback behavior.
- [ ] Add unit tests for report serialization and inventory edge cases.
- [ ] Add structured local logging and optional report redaction.
- [ ] Improve accessibility, interface scaling, and keyboard navigation.

## Phase 3 — Packaging and release engineering
- [ ] Choose a signed installer or MSIX packaging approach.
- [ ] Define versioning and release channels.
- [ ] Add artifact publishing to CI after build validation.
- [ ] Document update and rollback behavior.

## Phase 4 — Authorized integration research
- [ ] Review official Rockstar announcements and distribution requirements.
- [ ] Confirm legal and technical feasibility before considering integration.
- [ ] Document verified requirements only from an authorized PC release.
- [ ] Prototype only against authorized components.

## Release policy

No release will be described as a playable GTA VI PC port unless it genuinely provides that functionality through authorized game components. Until then, releases will be clearly labeled as independent Windows diagnostic tools or previews.
