using System.Security.Cryptography;
using System.Text;
using Volumix.Application;
using Volumix.Core;

namespace Volumix.Platform.Linux;

public sealed class ApplicationResolver(
    IProcessMetadataProvider processes,
    IDesktopApplicationIndex desktopApplications,
    ISteamApplicationResolver? steamApplications = null) : IApplicationResolver
{
    public async ValueTask<ApplicationIdentity> ResolveAsync(
        AudioSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ProcessMetadata? process = session.ProcessId is { } processId
            ? await processes.GetAsync(processId, cancellationToken).ConfigureAwait(false)
            : null;

        if (steamApplications is not null &&
            await steamApplications.TryResolveAsync(session, process, cancellationToken).ConfigureAwait(false) is { } steamIdentity)
        {
            return steamIdentity;
        }

        var evidence = BuildPipeWireEvidence(session);
        string? executablePath = process?.ExecutablePath;
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            evidence.Add(new(IdentityEvidenceKind.ProcessExecutable, executablePath, "Resolved from /proc/<pid>/exe."));
            IReadOnlyList<DesktopApplicationEntry> matches = desktopApplications.FindByExecutable(executablePath);
            if (matches.Count == 1)
            {
                DesktopApplicationEntry desktop = matches[0];
                evidence.Add(new(IdentityEvidenceKind.DesktopEntry, desktop.Id, "Executable matched one XDG desktop entry."));
                return new ApplicationIdentity
                {
                    Id = new($"xdg:{desktop.Id}"),
                    DisplayName = desktop.Name,
                    DesktopFileId = desktop.Id,
                    ExecutablePath = executablePath,
                    ExecutableName = Path.GetFileName(executablePath),
                    Icon = desktop.Icon is null ? IconFromPipeWire(session) : new(desktop.Icon),
                    Confidence = IdentityConfidence.High,
                    Evidence = evidence
                };
            }

            if (matches.Count > 1)
            {
                evidence.Add(new(IdentityEvidenceKind.AmbiguousDesktopEntry,
                    string.Join(',', matches.Select(match => match.Id)),
                    "Executable matched multiple desktop entries; no desktop identity was guessed."));
            }

            return ExecutableIdentity(session, executablePath, evidence);
        }

        string? binary = NullIfWhiteSpace(session.ProcessBinary);
        if (binary is not null)
        {
            IReadOnlyList<DesktopApplicationEntry> matches = desktopApplications.FindByExecutable(binary);
            if (matches.Count == 1)
            {
                DesktopApplicationEntry desktop = matches[0];
                evidence.Add(new(IdentityEvidenceKind.DesktopEntry, desktop.Id, "PipeWire process binary matched one XDG desktop entry."));
                return new ApplicationIdentity
                {
                    Id = new($"xdg:{desktop.Id}"),
                    DisplayName = desktop.Name,
                    DesktopFileId = desktop.Id,
                    ExecutableName = Path.GetFileName(binary),
                    Icon = desktop.Icon is null ? IconFromPipeWire(session) : new(desktop.Icon),
                    Confidence = IdentityConfidence.Medium,
                    Evidence = evidence
                };
            }
        }

        string? applicationId = NullIfWhiteSpace(session.ApplicationId);
        if (applicationId is not null)
        {
            return new ApplicationIdentity
            {
                Id = new($"pipewire:{NormalizeId(applicationId)}"),
                DisplayName = FirstNonEmpty(session.ApplicationName, binary is null ? null : Path.GetFileName(binary), session.MediaName,
                    "Unknown audio application"),
                ExecutableName = binary is null ? null : Path.GetFileName(binary),
                Icon = IconFromPipeWire(session),
                Confidence = IdentityConfidence.Medium,
                Evidence = evidence
            };
        }

        if (binary is not null)
        {
            return ExecutableIdentity(session, binary, evidence);
        }

        string displayName = FirstNonEmpty(session.ApplicationName, session.MediaName, "Unknown audio application");
        string stableEvidence = FirstNonEmpty(session.ApplicationName, session.MediaName, session.MediaRole, "unknown-audio-application");
        evidence.Add(new(IdentityEvidenceKind.Fallback, stableEvidence, "No stable executable or desktop identity was available."));
        return new ApplicationIdentity
        {
            Id = new($"unknown:{StableHash(stableEvidence)}"),
            DisplayName = displayName,
            Icon = IconFromPipeWire(session),
            Confidence = IdentityConfidence.Unknown,
            Evidence = evidence
        };
    }

    private static ApplicationIdentity ExecutableIdentity(
        AudioSession session,
        string executablePath,
        IReadOnlyList<IdentityEvidence> evidence) => new()
        {
            Id = new($"exe:{executablePath}"),
            DisplayName = FirstNonEmpty(Path.GetFileName(executablePath), session.ApplicationName, session.MediaName,
            "Unknown audio application"),
            ExecutablePath = Path.IsPathRooted(executablePath) ? executablePath : null,
            ExecutableName = Path.GetFileName(executablePath),
            Icon = IconFromPipeWire(session),
            Confidence = Path.IsPathRooted(executablePath) ? IdentityConfidence.Medium : IdentityConfidence.Low,
            Evidence = evidence
        };

    private static List<IdentityEvidence> BuildPipeWireEvidence(AudioSession session)
    {
        var evidence = new List<IdentityEvidence>();
        Add(IdentityEvidenceKind.PipeWireApplicationId, session.ApplicationId, "PipeWire application.id metadata.");
        Add(IdentityEvidenceKind.PipeWireApplicationName, session.ApplicationName, "PipeWire application.name metadata.");
        Add(IdentityEvidenceKind.PipeWireProcessBinary, session.ProcessBinary, "PipeWire application.process.binary metadata.");
        return evidence;

        void Add(IdentityEvidenceKind kind, string? value, string description)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                evidence.Add(new(kind, value, description));
            }
        }
    }

    private static IconReference? IconFromPipeWire(AudioSession session) =>
        NullIfWhiteSpace(session.ApplicationIconName) is { } icon ? new(icon) : null;

    private static string FirstNonEmpty(params string?[] candidates) =>
        candidates.First(candidate => !string.IsNullOrWhiteSpace(candidate))!;

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string NormalizeId(string value) => value.Trim().ToLowerInvariant();

    private static string StableHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim().ToLowerInvariant())))[..16].ToLowerInvariant();
}
