# Application identity

The resolver returns an `ApplicationIdentity`, confidence, and inspectable evidence. Persistent canonical IDs never contain PID.

The current M0 path uses:

1. PipeWire application/process metadata;
2. `/proc/<pid>/exe` when available;
3. executable basename/path;
4. one cached XDG desktop-entry index;
5. a stable hash of the best non-PID fallback evidence.

Examples include `xdg:org.mozilla.firefox`, `exe:/usr/bin/vlc`, and `unknown:<hash>`. Ambiguous desktop matches reduce confidence instead of guessing. Steam/Proton has an interface seam but no implementation yet.

Resolver changes require synthetic fixture tests and must preserve existing user mappings once persistence exists.
