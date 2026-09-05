# Application identity

The resolver returns an `ApplicationIdentity`, confidence, and inspectable evidence. Persistent canonical IDs never contain PID.

The current M0 path uses:

1. PipeWire application/process metadata;
2. `/proc/<pid>/exe` when available;
3. executable basename/path;
4. one cached XDG desktop-entry index;
5. a stable hash of the best non-PID fallback evidence.

Examples include `xdg:org.mozilla.firefox`, `exe:/usr/bin/vlc`, and `unknown:<hash>`. Ambiguous desktop matches reduce confidence instead of guessing.

M0.2 implements the Steam resolver seam. Relevant Wine/Windows processes supply a bounded allowlist of environment evidence. A cached index discovers libraries through `libraryfolders.vdf` and reads installed app manifests. Agreeing numeric Steam IDs, an indexed compatibility directory, and an installed manifest produce `steam:<app-id>` at High confidence. Conflicts or missing evidence fall back to the ordinary resolver. The index refreshes when the CLI is restarted; automatic refresh and broader Wine/native Steam coverage remain in milestone 0.4.

Resolver changes require synthetic fixture tests and must preserve existing user mappings once persistence exists.
