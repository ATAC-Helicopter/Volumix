# Terminology

- **Audio session:** A transient runtime PipeWire playback stream/node.
- **Application identity:** Stable evidence-based identity for a user-recognizable application.
- **Runtime application:** One logical application plus its current zero-or-more audio sessions.
- **Canonical ID:** Stable key such as `xdg:org.mozilla.firefox`; never a PID.
- **Mixer target:** A logical destination for commands, such as an application, master device, or dynamic role.
- **Backend generation:** Connection epoch used to reject stale events after reconnect.
- **Evidence:** Inspectable metadata supporting an identity decision.
