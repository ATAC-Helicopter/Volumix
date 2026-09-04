# Packaging

Packaging is intentionally deferred until the M0 backend and identity spike is reliable. Planned priority is portable tarball, Flatpak, Debian package, AppImage, then RPM when justified.

Package definitions must not vendor arbitrary system libraries, request root at runtime, or grant permissions beyond PipeWire/controller needs. Removing a package must not delete user configuration unless the user explicitly requests purge.
