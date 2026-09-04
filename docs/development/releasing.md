# Releasing

Volumix uses semantic versioning. A release must be built from a tag in CI, pass managed/native tests and package smoke checks, record its commit and ABI version, and publish checksums.

Before 1.0, validate supported desktop sessions, distributions, real application identities, PipeWire restart behavior, clean installation/removal, and documentation accuracy. No release pipeline exists during M0.
