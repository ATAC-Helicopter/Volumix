using Volumix.Core;

namespace Volumix.Application;

public interface IApplicationResolver
{
    ValueTask<ApplicationIdentity> ResolveAsync(AudioSession session, CancellationToken cancellationToken = default);
}

public sealed record ProcessMetadata(int ProcessId, string? ExecutablePath, IReadOnlyList<string> Arguments);

public interface IProcessMetadataProvider
{
    ValueTask<ProcessMetadata?> GetAsync(int processId, CancellationToken cancellationToken = default);
}

public sealed record DesktopApplicationEntry(
    string Id,
    string Name,
    string? Executable,
    string? Icon,
    string? StartupWmClass,
    bool NoDisplay,
    bool Hidden,
    string SourcePath);

public interface IDesktopApplicationIndex
{
    IReadOnlyList<DesktopApplicationEntry> FindByExecutable(string executablePath);
}

public interface ISteamApplicationResolver
{
    ValueTask<ApplicationIdentity?> TryResolveAsync(
        AudioSession session,
        ProcessMetadata? process,
        CancellationToken cancellationToken = default);
}
