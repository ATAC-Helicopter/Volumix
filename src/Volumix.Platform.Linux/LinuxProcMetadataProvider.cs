using Volumix.Application;

namespace Volumix.Platform.Linux;

public sealed class LinuxProcMetadataProvider(string procRoot = "/proc") : IProcessMetadataProvider
{
    public ValueTask<ProcessMetadata?> GetAsync(int processId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (processId <= 0)
        {
            return ValueTask.FromResult<ProcessMetadata?>(null);
        }

        string directory = Path.Combine(procRoot, processId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        try
        {
            string? executablePath = ResolveExecutable(Path.Combine(directory, "exe"));
            IReadOnlyList<string> arguments = ReadNullSeparated(Path.Combine(directory, "cmdline"));
            return ValueTask.FromResult<ProcessMetadata?>(new(processId, executablePath, arguments));
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException or
                                          UnauthorizedAccessException or IOException)
        {
            // A stream's process can legitimately exit between the PipeWire event and inspection.
            return ValueTask.FromResult<ProcessMetadata?>(null);
        }
    }

    private static string? ResolveExecutable(string path)
    {
        FileSystemInfo? target = File.ResolveLinkTarget(path, returnFinalTarget: true);
        return target?.FullName;
    }

    private static IReadOnlyList<string> ReadNullSeparated(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        return System.Text.Encoding.UTF8.GetString(bytes)
            .Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }
}
