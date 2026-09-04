# Controller system

Physical controls will target the same `MixerTarget` and command path as UI, CLI, and D-Bus. Hardware layout, raw messages, semantic bindings, and feedback are separate concepts.

MIDI through ALSA Sequencer is the first planned backend. Absolute controls default to pickup mode to prevent jumps. Feedback-capable devices require origin-aware loop suppression. No controller implementation exists in M0.
