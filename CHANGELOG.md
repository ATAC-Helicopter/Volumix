# Changelog

All notable changes will be documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and releases will use semantic versioning.

## [Unreleased]

### Added

- `VMX-0014` Initial Steam/Proton identification using bounded process evidence, cached library/manifests, conflict fixtures, and a read-only Euro Truck Simulator 2 observation.

- `VMX-0011` Hardware-free PipeWire integration coverage for application discovery, multi-session control, removal, daemon restart, and managed reconnect.
- `VMX-0012` Deterministic browser fixtures and a real Brave multi-stream qualification covering XDG identity, grouping, command fan-out, removal, and recreation.
- `VMX-0015` End-to-end PipeWire restart qualification covering unavailable state, bounded reconnect, registry replacement, stale-event rejection, and recovered commands.
- `VMX-0016` ASAN/UBSAN CI coverage for native lifecycle, callback ownership, rapid node churn, command teardown, and PipeWire reconnect.
- `VMX-0018` Theme-aware repository wordmarks and curated public presentation artwork for the FG Labs project page.

### Changed

- `VMX-0012` Exact PipeWire desktop-ID hints can corroborate installed applications whose launchers wrap a differently named runtime executable.

### Fixed

- `BUG-00001` Roadmap synchronization can update managed issues after their GitHub milestone is closed.
- `BUG-00002` Native volume and mute commands now complete a PipeWire round trip before short-lived clients tear down their connection.

## [0.1.0-m0] - 2026-09-04

### Added

- `VMX-0001` M0 repository scaffold, .NET 10 solution, governance, licensing, documentation, and CI.
- `VMX-0002` Application-centric domain model and stable identity contracts.
- `VMX-0003` Native PipeWire C bridge with playback discovery and volume/mute commands.
- `VMX-0004` Managed native event copying and serialized event channel.
- `VMX-0005` Linux `/proc` and cached XDG application identity resolution.
- `VMX-0006` Multi-session application aggregation and immutable mixer coordination.
- `VMX-0007` Diagnostic CLI application listing, volume, and mute commands.
- `VMX-0008` Minimal Avalonia shell and theme infrastructure.
- `VMX-0010` Managed reconnect supervision and controlled multi-session qualification.
- `REL-00001` M0 build, test, native lifecycle, and live PipeWire evidence baseline.

[0.1.0-m0]: https://github.com/ATAC-Helicopter/Volumix/releases/tag/v0.1.0-m0
