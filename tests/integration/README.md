# Integration tests

Integration tests may create controlled local PipeWire streams but must not alter unrelated sessions. They need deterministic cleanup and must be opt-in when they require a user audio service.

Run the isolated PipeWire topology and control test after building:

```bash
VOLUMIX_RUN_PIPEWIRE_INTEGRATION=1 ./scripts/test.sh
```

The harness replaces `XDG_RUNTIME_DIR`, starts its own `pipewire -c minimal.conf`, creates two unlinked silent streams, verifies grouping, volume and mute, and terminates only the exact processes it created.
