namespace Volumix.Platform.Linux.Tests;

public sealed class LinuxProcMetadataProviderTests
{
    [Fact]
    public async Task ReadsSyntheticProcFixture()
    {
        string root = Path.Combine(Path.GetTempPath(), $"volumix-proc-{Guid.NewGuid():N}");
        string processDirectory = Path.Combine(root, "123");
        Directory.CreateDirectory(processDirectory);
        try
        {
            string executable = Path.Combine(root, "test-player");
            await File.WriteAllTextAsync(executable, "fixture", TestContext.Current.CancellationToken);
            File.CreateSymbolicLink(Path.Combine(processDirectory, "exe"), executable);
            await File.WriteAllBytesAsync(Path.Combine(processDirectory, "cmdline"),
                "test-player\0--play\0"u8.ToArray(), TestContext.Current.CancellationToken);
            var provider = new LinuxProcMetadataProvider(root);
            Volumix.Application.ProcessMetadata? result = await provider.GetAsync(123, TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.Equal(executable, result.ExecutablePath);
            Assert.Equal(["test-player", "--play"], result.Arguments);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MissingProcessReturnsNull()
    {
        var provider = new LinuxProcMetadataProvider("/definitely/not/a/proc/filesystem");
        Assert.Null(await provider.GetAsync(123, TestContext.Current.CancellationToken));
    }
}
