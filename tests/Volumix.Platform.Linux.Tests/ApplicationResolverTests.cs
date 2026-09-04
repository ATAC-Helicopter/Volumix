using Volumix.Application;
using Volumix.Core;
using Volumix.Infrastructure;
using ApplicationId = Volumix.Core.ApplicationId;

namespace Volumix.Platform.Linux.Tests;

public sealed class ApplicationResolverTests
{
    private static string DesktopFixtures => Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../tests/fixtures/desktop-files"));

    [Fact]
    public async Task ResolvesNormalNativeExecutableToDesktopEntry()
    {
        var resolver = Resolver(new ProcessMetadata(42, "/usr/lib/firefox/firefox", ["firefox"]));
        ApplicationIdentity identity = await resolver.ResolveAsync(Session(processId: 42), TestContext.Current.CancellationToken);
        Assert.Equal(new ApplicationId("xdg:firefox"), identity.Id);
        Assert.Equal(IdentityConfidence.High, identity.Confidence);
    }

    [Fact]
    public async Task MissingProcProcessFallsBackToPipeWireIdentity()
    {
        var resolver = Resolver(process: null);
        ApplicationIdentity identity = await resolver.ResolveAsync(Session(processId: 999, applicationId: "org.example.Player"), TestContext.Current.CancellationToken);
        Assert.Equal(new ApplicationId("pipewire:org.example.player"), identity.Id);
        Assert.DoesNotContain("999", identity.Id.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProcessDisappearingDuringLookupIsNormal()
    {
        var resolver = new ApplicationResolver(new VanishingProcessProvider(), new EmptyDesktopIndex());
        ApplicationIdentity identity = await resolver.ResolveAsync(Session(processId: 15, applicationId: null), TestContext.Current.CancellationToken);
        Assert.StartsWith("unknown:", identity.Id.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AmbiguousDesktopEntryUsesExecutableIdentity()
    {
        var resolver = new ApplicationResolver(
            new FixedProcessProvider(new(7, "/usr/bin/player", [])),
            new AmbiguousDesktopIndex());
        ApplicationIdentity identity = await resolver.ResolveAsync(Session(processId: 7), TestContext.Current.CancellationToken);
        Assert.Equal(new ApplicationId("exe:/usr/bin/player"), identity.Id);
        Assert.Contains(identity.Evidence, evidence => evidence.Kind == IdentityEvidenceKind.AmbiguousDesktopEntry);
    }

    [Fact]
    public async Task UnmatchedExecutableHasStableExecutableIdentity()
    {
        var resolver = new ApplicationResolver(
            new FixedProcessProvider(new(8, "/opt/example/audio-player", [])),
            new EmptyDesktopIndex());
        ApplicationIdentity identity = await resolver.ResolveAsync(Session(processId: 8), TestContext.Current.CancellationToken);
        Assert.Equal(new ApplicationId("exe:/opt/example/audio-player"), identity.Id);
        Assert.Equal("audio-player", identity.DisplayName);
    }

    [Fact]
    public async Task ExactDesktopIdHintCorroboratesWrapperExecutable()
    {
        var resolver = new ApplicationResolver(
            new FixedProcessProvider(new(9, "/opt/brave.com/brave/brave", ["brave", "--type=utility"])),
            new XdgDesktopApplicationIndex([DesktopFixtures]));
        AudioSession session = Session(processId: 9, applicationId: null) with
        {
            ApplicationName = "Brave",
            ApplicationIconName = "brave-browser",
            ProcessBinary = "brave"
        };

        ApplicationIdentity identity = await resolver.ResolveAsync(session, TestContext.Current.CancellationToken);

        Assert.Equal(new ApplicationId("xdg:brave-browser"), identity.Id);
        Assert.Equal("Brave Web Browser", identity.DisplayName);
        Assert.Equal(IdentityConfidence.High, identity.Confidence);
        Assert.Contains(identity.Evidence, evidence =>
            evidence.Kind == IdentityEvidenceKind.DesktopEntry && evidence.Value == "brave-browser");
    }

    [Fact]
    public async Task UnknownIdentityHashDoesNotDependOnPid()
    {
        var resolver = Resolver(process: null);
        ApplicationIdentity first = await resolver.ResolveAsync(Session(processId: 1, applicationId: null), TestContext.Current.CancellationToken);
        ApplicationIdentity second = await resolver.ResolveAsync(Session(processId: 2, applicationId: null), TestContext.Current.CancellationToken);
        Assert.Equal(first.Id, second.Id);
    }

    private static ApplicationResolver Resolver(ProcessMetadata? process) => new(
        new FixedProcessProvider(process), new XdgDesktopApplicationIndex([DesktopFixtures]));

    private static AudioSession Session(int? processId, string? applicationId = "firefox") => new()
    {
        Id = new("fixture"),
        PipeWireNodeId = 81,
        ProcessId = processId,
        ApplicationId = applicationId,
        ApplicationName = "Firefox",
        MediaName = "Fixture audio",
        Volume = 0.5f,
        Active = true
    };

    private sealed class FixedProcessProvider(ProcessMetadata? process) : IProcessMetadataProvider
    {
        public ValueTask<ProcessMetadata?> GetAsync(int processId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(process);
    }

    private sealed class VanishingProcessProvider : IProcessMetadataProvider
    {
        public ValueTask<ProcessMetadata?> GetAsync(int processId, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<ProcessMetadata?>(null);
    }

    private sealed class EmptyDesktopIndex : IDesktopApplicationIndex
    {
        public IReadOnlyList<DesktopApplicationEntry> FindByExecutable(string executablePath) => [];
        public IReadOnlyList<DesktopApplicationEntry> FindById(string desktopFileId) => [];
    }

    private sealed class AmbiguousDesktopIndex : IDesktopApplicationIndex
    {
        public IReadOnlyList<DesktopApplicationEntry> FindByExecutable(string executablePath) =>
        [
            new("one", "One", executablePath, null, null, false, false, "one.desktop"),
            new("two", "Two", executablePath, null, null, false, false, "two.desktop")
        ];

        public IReadOnlyList<DesktopApplicationEntry> FindById(string desktopFileId) => [];
    }
}
