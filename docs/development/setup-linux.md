# Linux development setup

Volumix requires the .NET 10 SDK, a C11 compiler, CMake, pkg-config, PipeWire/SPA development headers, and ALSA development headers.

On Debian and Ubuntu family systems:

```bash
sudo apt install \
  build-essential \
  cmake \
  ninja-build \
  pkg-config \
  libpipewire-0.3-dev \
  libspa-0.2-dev \
  libasound2-dev
```

Install the .NET 10 SDK using the supported Microsoft or distribution instructions. The repository pins SDK feature band 10.0.300 and rolls forward to a newer patch in that band.

The app connects to the current user's PipeWire instance. It does not require root or a system service.
