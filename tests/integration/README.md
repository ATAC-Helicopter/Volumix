# Integration tests

Integration tests may create controlled local PipeWire streams but must not alter unrelated sessions. They need deterministic cleanup and must be opt-in when they require a user audio service.
