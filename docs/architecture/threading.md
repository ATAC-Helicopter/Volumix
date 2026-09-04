# Threading and lifecycle

PipeWire callbacks run on the native thread loop. They must do bounded work and never call UI code. Native values are copied into managed records and written to a channel.

`MixerStateCoordinator` is the sole owner of mutable runtime session/identity state. Its event stream has one consumer and produces immutable snapshots. Presentation dispatching belongs at the UI boundary.

Shutdown order is: stop accepting commands, cancel event consumption, dispose the current backend, stop the PipeWire loop, destroy the native context, then exit.
