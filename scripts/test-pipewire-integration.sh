#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

for command in pipewire pw-cat dotnet rg; do
    if ! command -v "$command" >/dev/null 2>&1; then
        echo "Skipping isolated PipeWire integration test: missing $command." >&2
        exit 77
    fi
done

runtime_directory="$(mktemp -d -t volumix-pipewire-XXXXXX)"
pipewire_pid=""
first_stream_pid=""
second_stream_pid=""
watcher_pid=""

cleanup() {
    for process_id in "$watcher_pid" "$first_stream_pid" "$second_stream_pid" "$pipewire_pid"; do
        if [[ -n "$process_id" ]]; then
            kill "$process_id" 2>/dev/null || true
        fi
    done
    wait 2>/dev/null || true
    rm -rf -- "$runtime_directory"
}
trap cleanup EXIT INT TERM

export XDG_RUNTIME_DIR="$runtime_directory"
export PIPEWIRE_RUNTIME_DIR="$runtime_directory"
export PIPEWIRE_REMOTE="pipewire-0"

start_pipewire() {
    local log_path="$1"
    pipewire -c minimal.conf >"$log_path" 2>&1 &
    pipewire_pid=$!

    for _ in {1..100}; do
        if [[ -S "$runtime_directory/pipewire-0" ]]; then
            return
        fi
        if ! kill -0 "$pipewire_pid" 2>/dev/null; then
            echo "Isolated PipeWire exited during startup:" >&2
            sed -n '1,160p' "$log_path" >&2
            exit 1
        fi
        sleep 0.05
    done

    echo "Timed out waiting for the isolated PipeWire socket." >&2
    exit 1
}

wait_for_pattern() {
    local file_path="$1"
    local pattern="$2"
    for _ in {1..150}; do
        if rg -F "$pattern" "$file_path" >/dev/null 2>&1; then
            return
        fi
        sleep 0.1
    done
    echo "Timed out waiting for '$pattern' in $file_path." >&2
    sed -n '1,240p' "$file_path" >&2
    exit 1
}

start_pipewire "$runtime_directory/pipewire-initial.log"

stream_properties='{ application.name = "Volumix Integration Fixture" application.id = "dev.fglabs.Volumix.IntegrationFixture" }'
pw-cat --playback --target 0 --rate 48000 --channels 2 --format s16 --volume 0.8 \
    -P "$stream_properties" - </dev/zero >"$runtime_directory/stream-one.log" 2>&1 &
first_stream_pid=$!
pw-cat --playback --target 0 --rate 48000 --channels 2 --format s16 --volume 0.8 \
    -P "$stream_properties" - </dev/zero >"$runtime_directory/stream-two.log" 2>&1 &
second_stream_pid=$!

sleep 0.25

cli_project="$repository_root/src/Volumix.Cli/Volumix.Cli.csproj"
initial="$(dotnet run --project "$cli_project" --no-build -- apps)"
printf '%s\n' "$initial" | rg -F "Canonical ID: pipewire:dev.fglabs.volumix.integrationfixture" >/dev/null
printf '%s\n' "$initial" | rg -F "Sessions: 2" >/dev/null
printf '%s\n' "$initial" | rg -F "Volume: 100%" >/dev/null

dotnet run --project "$cli_project" --no-build -- \
    set pipewire:dev.fglabs.volumix.integrationfixture 35 >/dev/null
after_volume="$(dotnet run --project "$cli_project" --no-build -- apps)"
printf '%s\n' "$after_volume" | rg -F "Sessions: 2" >/dev/null
printf '%s\n' "$after_volume" | rg -F "Volume: 35%" >/dev/null

dotnet run --project "$cli_project" --no-build -- \
    mute pipewire:dev.fglabs.volumix.integrationfixture >/dev/null
after_mute="$(dotnet run --project "$cli_project" --no-build -- apps)"
printf '%s\n' "$after_mute" | rg -F "Muted: true" >/dev/null

kill "$second_stream_pid"
wait "$second_stream_pid" 2>/dev/null || true
second_stream_pid=""
after_removal="$(dotnet run --project "$cli_project" --no-build -- apps)"
printf '%s\n' "$after_removal" | rg -F "Sessions: 1" >/dev/null

dotnet run --project "$cli_project" --no-build -- apps --watch \
    >"$runtime_directory/watch.log" 2>&1 &
watcher_pid=$!
wait_for_pattern "$runtime_directory/watch.log" "Canonical ID: pipewire:dev.fglabs.volumix.integrationfixture"

kill "$first_stream_pid"
wait "$first_stream_pid" 2>/dev/null || true
first_stream_pid=""
kill "$pipewire_pid"
wait "$pipewire_pid" 2>/dev/null || true
pipewire_pid=""
rm -f -- "$runtime_directory/pipewire-0"

start_pipewire "$runtime_directory/pipewire-restarted.log"

restart_properties='{ application.name = "Volumix Restart Fixture" application.id = "dev.fglabs.Volumix.RestartFixture" }'
pw-cat --playback --target 0 --rate 48000 --channels 2 --format s16 --volume 0.8 \
    -P "$restart_properties" - </dev/zero >"$runtime_directory/stream-restart.log" 2>&1 &
first_stream_pid=$!

wait_for_pattern "$runtime_directory/watch.log" "Canonical ID: pipewire:dev.fglabs.volumix.restartfixture"
if ! kill -0 "$watcher_pid" 2>/dev/null; then
    echo "The watching client exited instead of reconnecting." >&2
    exit 1
fi

after_restart="$(dotnet run --project "$cli_project" --no-build -- apps)"
printf '%s\n' "$after_restart" | rg -F "Canonical ID: pipewire:dev.fglabs.volumix.restartfixture" >/dev/null
printf '%s\n' "$after_restart" | rg -F "Sessions: 1" >/dev/null

echo "Isolated PipeWire integration test passed."
