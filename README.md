<p align="center">
  <img src="assets/branding/source/volumix-icon-master.svg" width="112" alt="Volumix logo: three vertical volume faders">
</p>

<h1 align="center">Volumix</h1>

<p align="center">
  A Linux volume mixer that controls applications—not raw audio streams.
</p>

<p align="center">
  <a href="LICENSE"><img alt="License: GPL-3.0-or-later" src="https://img.shields.io/badge/license-GPL--3.0--or--later-6f42c1.svg"></a>
  <img alt="Platform: Linux" src="https://img.shields.io/badge/platform-Linux-fcc624.svg">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512bd4.svg">
  <img alt="Status: M0 spike" src="https://img.shields.io/badge/status-M0%20technical%20spike-orange.svg">
</p>

Volumix resolves PipeWire playback nodes into stable logical applications and groups every session owned by an application behind one mixer target. Three Firefox streams should look like one **Firefox** control—not three nodes, renderer processes, or PIDs.

> [!IMPORTANT]
> Volumix is an early technical spike, not a release-ready mixer. Packaging, persistence, Steam/Proton resolution, MIDI control, and the finished interface are not implemented yet.

## What works today

- Native PipeWire registry connection and playback-node events.
- Versioned C ABI with copied, serialized managed events.
- Linux `/proc` and cached XDG desktop-entry identity resolution.
- Stable fallback identities that never use PID.
- Multi-session application grouping and application-wide volume/mute commands.
- Managed reconnect supervision with generation isolation.
- Diagnostic CLI and minimal Avalonia shell.

## Architecture

```text
PipeWire sessions
    ↓
Application identity resolver
    ↓
Logical applications
    ↓
Mixer state
    ↓
UI / MIDI / CLI / D-Bus
```

The complete product contract is in [TECH_SPEC.md](TECH_SPEC.md). Start with the shorter [architecture overview](docs/architecture/overview.md) when contributing.

Planning is tracked in the canonical [ROADMAP.md](ROADMAP.md) and mirrored to the private [Volumix Roadmap GitHub Project](https://github.com/users/ATAC-Helicopter/projects/9).

## Build

Install the dependencies in [the Linux setup guide](docs/development/setup-linux.md), then run:

```bash
./scripts/build.sh
./scripts/test.sh
```

Inspect current logical applications:

```bash
dotnet run --project src/Volumix.Cli -- apps --watch
```

Exercise the shared M0 command path:

```bash
dotnet run --project src/Volumix.Cli -- set <canonical-id> 35
dotnet run --project src/Volumix.Cli -- mute <canonical-id>
dotnet run --project src/Volumix.Cli -- unmute <canonical-id>
```

These commands currently open a short-lived PipeWire client. A later milestone will route them through the single running instance over D-Bus.

## Scope

Volumix is not a PipeWire graph editor, DAW, DSP host, recorder, virtual-device manager, or network audio server. See [product scope](docs/product/scope.md).

## Contributing and security

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md), the repository-specific [agent rules](AGENTS.md), and the [Code of Conduct](CODE_OF_CONDUCT.md). Report vulnerabilities using [SECURITY.md](SECURITY.md), not a public issue.

## License

Copyright © 2026 Volumix contributors. Volumix is free software licensed under [GPL-3.0-or-later](LICENSE). Third-party components retain their respective licenses; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
