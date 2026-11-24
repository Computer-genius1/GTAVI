# GTA VI for Windows 11

An independent community project exploring the groundwork for a Windows 11 experience for Grand Theft Auto VI (GTA VI).

> [!IMPORTANT]
> This repository is **not an official Rockstar Games project**, is not endorsed by Rockstar Games, and is not affiliated with Take-Two Interactive. It does not contain GTA VI game assets, executable files, source code, or a playable PC port. A genuine native PC release or port requires authorized game technology and permission from the rights holders. This project will not bypass platform protections or distribute copyrighted game content.

## Project goal

The long-term goal is to investigate and develop the legitimate Windows-side components that could support a Windows 11 release of GTA VI, subject to the availability of an authorized PC build, licensing, and technical feasibility.

The project is intentionally scoped to **Windows 11**. Windows 10 is not a target platform.

## Project status

<badge>Early planning / groundwork</badge>

The repository is at its foundation stage. Initial work focuses on documentation, project structure, development standards, and a clear technical plan. There is currently no playable game, installer, launcher, compatibility layer, or performance patch.

## Platform direction

- **Operating system:** Windows 11 only.
- **Primary architecture:** x64 (AMD64).
- **Additional architecture:** ARM64 research only; support will depend on official game availability and compatible dependencies.
- **Development approach:** Windows-native components, with clear separation between application code and any future authorized game integration.
- **Distribution:** No game binaries or Rockstar-owned assets will be redistributed.

## Planned areas of work

1. **Project foundation** — repository structure, contribution guidance, security reporting, and development documentation.
2. **Windows 11 application shell** — a native application foundation for settings, diagnostics, and future authorized integration.
3. **System compatibility** — Windows version, CPU architecture, memory, graphics, storage, and driver checks.
4. **Diagnostics and logging** — useful, privacy-conscious reports to help troubleshoot supported configurations.
5. **Performance research** — evidence-based testing and optimization only where technically and legally possible.
6. **Authorized game integration** — investigate integration only if Rockstar provides or authorizes the required PC game components.

## Repository layout

```text
GTAVI/
├── docs/
│   ├── ARCHITECTURE.md
│   └── ROADMAP.md
├── .editorconfig
├── .gitignore
├── CONTRIBUTING.md
├── SECURITY.md
└── README.md
```

## Requirements

There is no runnable application yet. The eventual target is a supported Windows 11 installation. Specific minimum hardware requirements will not be published until there is an authorized game build and a tested application.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening an issue or pull request. For security-sensitive reports, follow [SECURITY.md](SECURITY.md).

## Legal and trademarks

Grand Theft Auto, GTA VI, and related names and marks are the property of their respective owners. They are used here only to identify the subject of this independent community project. This repository does not claim ownership of those marks or any Rockstar Games intellectual property.

## Disclaimer

This is an experimental, independent project. No compatibility, release date, performance level, or playable PC version is promised. Any future integration must use lawfully obtained and authorized components.
