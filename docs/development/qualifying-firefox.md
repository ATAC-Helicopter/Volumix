# Qualifying Firefox identity

`VMX-0012` owns Firefox multi-stream qualification. It is intentionally split into deterministic automated evidence and a real-browser observation; the ticket remains open until both exist.

## Automated evidence

The isolated PipeWire harness runs copied `pw-cat` clients under a `firefox` executable name and supplies the synthetic `firefox.desktop` fixture through an isolated `XDG_DATA_HOME`. This is not Firefox. It exists to exercise the same real Linux boundaries deterministically:

- playback nodes arrive through the native PipeWire bridge;
- `/proc/<pid>/exe` resolves each fixture process;
- the executable is corroborated against the XDG desktop index;
- two streams group under `xdg:firefox` with High confidence and the Firefox icon;
- volume and mute commands reach every grouped node;
- removing and recreating a stream preserves one canonical application identity.

Run it after building with:

```bash
VOLUMIX_RUN_PIPEWIRE_INTEGRATION=1 ./scripts/test.sh
```

## Remaining real-browser observation

On a desktop with Firefox and a normal user PipeWire service:

1. Start `volumixctl apps --watch`.
2. Start Firefox and play two independent audio sources.
3. Confirm exactly one Firefox application with `Canonical ID: xdg:firefox`, more than one session, High confidence, and the installed Firefox icon.
4. Set volume and mute through `volumixctl`; confirm every Firefox source follows and unrelated applications do not.
5. Stop one source; confirm the Firefox application remains with one fewer session.
6. Recreate that source; confirm it rejoins the same canonical application.
7. Restart Firefox; confirm the canonical identity remains stable.

Record desktop environment, distribution, Firefox packaging type, Firefox version, and observed output without committing PIDs, media titles, full command lines, or environment dumps.
