# Architecture

## Current state

This document describes a proposed architecture. No application implementation exists yet.

## Design principles

- Target Windows 11 only; do not add Windows 10 compatibility paths.
- Keep the Windows-side application separate from proprietary game code and assets.
- Use only documented, supported Windows APIs and lawfully obtained dependencies.
- Do not circumvent DRM, anti-cheat, platform security, or other access controls.
- Keep diagnostics transparent and privacy-conscious; collect no telemetry by default.
- Make architecture-specific behavior explicit rather than assuming x64 and ARM64 are interchangeable.

## Proposed components

### 1. Windows application shell

A small native Windows application may eventually provide:
- System and build information.
- Compatibility and prerequisite checks.
- User-configurable settings.
- Local diagnostic logs and export controls.
- Links to official sources and support documentation.

### 2. Compatibility assessment

The assessment layer should report, without modifying the system:
- Windows edition, version, and build.
- Process and operating-system architecture.
- CPU feature availability.
- Installed memory and available storage.
- GPU identity, driver version, and supported graphics capabilities.

Checks must distinguish verified requirements from advisory recommendations. Do not invent GTA VI requirements before an authorized PC build publishes them.

### 3. Authorized game integration

This layer is deliberately unimplemented. It can be designed only after lawful access to an official PC build and any required authorization. It must not include copied game assets, proprietary code, DRM circumvention, or unofficial executable modifications.

### 4. Diagnostics

- Store logs locally by default.
- Avoid collecting personal files, account credentials, or unrelated system data.
- Let users inspect and explicitly export diagnostic reports.
- Redact sensitive values from reports where practical.

## Technology selection

The implementation framework has not yet been locked. Before choosing it, compare supported Windows 11 desktop frameworks, packaging, accessibility, update mechanisms, and x64/ARM64 support. Record the decision and rationale here before introducing a large dependency stack.
