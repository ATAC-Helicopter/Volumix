using System.Runtime.CompilerServices;
using Volumix.Core;

namespace Volumix.Application.Tests;

public sealed class ReconnectingAudioBackendTests
{
    [Fact]
    public async Task ReconnectsWithANewGenerationAfterDisconnect()
    {
        int created = 0;
        await using var backend = new ReconnectingAudioBackend(
            () => new FixtureBackend(++created), TimeSpan.Zero, TimeSpan.Zero);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(2));
        var events = new List<AudioBackendEvent>();

        await foreach (AudioBackendEvent backendEvent in backend.WatchAsync(timeout.Token))
        {
            events.Add(backendEvent);
            if (events.OfType<SessionAdded>().Count() == 2)
            {
                break;
            }
        }

        Assert.Equal([1L, 2L], events.OfType<BackendReady>().Select(item => item.Generation));
        Assert.Contains(events, item => item is BackendDisconnected { Generation: 1 });
        Assert.Equal(
            [new AudioSessionId("backend:1:81"), new AudioSessionId("backend:2:81")],
            events.OfType<SessionAdded>().Select(item => item.Session.Id));
        Assert.Equal(2, created);
    }

    private sealed class FixtureBackend(int instance) : IAudioBackend
    {
        public async IAsyncEnumerable<AudioBackendEvent> WatchAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new BackendReady(99);
            yield return new SessionAdded(99, new AudioSession
            {
                Id = new("pipewire:99:81"),
                PipeWireNodeId = 81,
                Volume = 1f,
                Active = true
            });
            if (instance == 1)
            {
                yield return new BackendDisconnected(99, "fixture disconnect");
            }
        }

        public ValueTask SetStreamVolumeAsync(uint nodeId, float volume, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask SetStreamMuteAsync(uint nodeId, bool muted, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
