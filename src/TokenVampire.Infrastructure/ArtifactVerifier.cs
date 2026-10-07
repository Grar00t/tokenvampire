using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace TokenVampire.Infrastructure;

public enum ArtifactVerificationStatus
{
    Match,
    Missing,
    LinkRejected,
    SizeMismatch,
    HashMismatch,
    Unreadable,
    InvalidExpectation,
}

public sealed record ArtifactVerificationResult(
    ArtifactVerificationStatus Status,
    string Path,
    long? ObservedBytes = null,
    string? ObservedSha256 = null,
    string? Message = null)
{
    public bool Matched => Status == ArtifactVerificationStatus.Match;
}

public static class ArtifactVerifier
{
    public static ArtifactVerificationResult Verify(
        string path,
        string? expectedSha256 = null,
        long? expectedBytes = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Invalid(path ?? string.Empty, "path is required");

        if (expectedBytes is < 0)
            return Invalid(path, "expected byte length must be non-negative");

        string? expectedHash = null;
        if (expectedSha256 is not null)
        {
            var candidate = expectedSha256.Trim();
            if (candidate.Length != 64 || !IsHex(candidate))
                return Invalid(path, "expected SHA-256 must be exactly 64 hexadecimal characters");

            expectedHash = candidate.ToLowerInvariant();
        }

        try
        {
            using var stream = SecureArtifactFile.OpenRead(path);
            var observedBytes = stream.Length;
            var observedHash = Convert.ToHexStringLower(SHA256.HashData(stream));

            if (expectedBytes is not null && observedBytes != expectedBytes.Value)
                return new(ArtifactVerificationStatus.SizeMismatch, path, observedBytes, observedHash,
                    $"expected {expectedBytes.Value} bytes but observed {observedBytes}");

            if (expectedHash is not null && !string.Equals(observedHash, expectedHash, StringComparison.Ordinal))
                return new(ArtifactVerificationStatus.HashMismatch, path, observedBytes, observedHash,
                    $"expected SHA-256 {expectedHash} but observed {observedHash}");

            return new(ArtifactVerificationStatus.Match, path, observedBytes, observedHash);
        }
        catch (ArtifactLinkException e)
        {
            return new(ArtifactVerificationStatus.LinkRejected, path, Message: e.Message);
        }
        catch (FileNotFoundException)
        {
            return new(ArtifactVerificationStatus.Missing, path, Message: "artifact does not exist");
        }
        catch (DirectoryNotFoundException)
        {
            return new(ArtifactVerificationStatus.Missing, path, Message: "artifact does not exist");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or Win32Exception)
        {
            return new(ArtifactVerificationStatus.Unreadable, path, Message: e.Message);
        }
    }

    static ArtifactVerificationResult Invalid(string path, string message) =>
        new(ArtifactVerificationStatus.InvalidExpectation, path, Message: message);

    static bool IsHex(string value)
    {
        foreach (var c in value)
        {
            if (c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')
                continue;
            return false;
        }
        return true;
    }
}

sealed class ArtifactLinkException(string message) : IOException(message);

static class SecureArtifactFile
{
    const int S_IFMT = 0xF000;
    const int S_IFREG = 0x8000;

    public static FileStream OpenRead(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return OperatingSystem.IsWindows()
            ? OpenWindows(fullPath)
            : OpenUnix(fullPath);
    }

    static FileStream OpenWindows(string fullPath)
    {
        if (fullPath.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase) ||
            fullPath.StartsWith(@"\\?\GLOBALROOT", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("device namespace paths are not accepted as artifact evidence");
        }

        RejectReparseComponentsWindows(fullPath);

        SafeFileHandle? handle = null;
        try
        {
            handle = File.OpenHandle(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan);

            if (GetFileType(handle) != 1)
                throw new IOException("artifact is not a regular disk file");

            var handleAttributes = File.GetAttributes(handle);
            if ((handleAttributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                throw new ArtifactLinkException("symbolic links, reparse points, and directories are not accepted as direct artifact evidence");

            var resolved = GetWindowsHandlePath(handle);
            if (!PathEqualsWindows(fullPath, resolved))
                throw new ArtifactLinkException("artifact path resolved through a reparse point");

            RejectReparseComponentsWindows(fullPath);

            var stream = new FileStream(handle, FileAccess.Read);
            handle = null;
            return stream;
        }
        finally
        {
            handle?.Dispose();
        }
    }

    static void RejectReparseComponentsWindows(string fullPath)
    {
        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrEmpty(root))
            throw new IOException("artifact path has no filesystem root");

        var relative = fullPath[root.Length..];
        var current = root;
        foreach (var component in relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            var attributes = File.GetAttributes(current);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new ArtifactLinkException("artifact path contains a symbolic link or reparse point");
        }
    }

    static string GetWindowsHandlePath(SafeFileHandle handle)
    {
        var capacity = 512;
        while (true)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Capacity, 0);
            if (length == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            if (length < buffer.Capacity)
                return NormalizeWindowsHandlePath(buffer.ToString());

            capacity = checked((int)length + 1);
        }
    }

    static string NormalizeWindowsHandlePath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            return @"\\" + path[8..];

        if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
            return path[4..];

        return path;
    }

    static bool PathEqualsWindows(string expected, string observed) =>
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(expected)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(observed)),
            StringComparison.OrdinalIgnoreCase);

    static FileStream OpenUnix(string fullPath)
    {
        var components = fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        if (components.Length == 0)
            throw new IOException("artifact path does not identify a regular file");

        var oDirectory = OperatingSystem.IsMacOS() ? 0x100000 : 0x10000;
        var oNoFollow = OperatingSystem.IsMacOS() ? 0x0100 : 0x20000;
        var oNonBlock = OperatingSystem.IsMacOS() ? 0x0004 : 0x0800;

        var directoryFd = open("/", oDirectory | oNoFollow);
        if (directoryFd < 0)
            ThrowUnixOpenError(Marshal.GetLastPInvokeError(), fullPath);

        try
        {
            for (var i = 0; i < components.Length - 1; i++)
            {
                var nextFd = openat(directoryFd, components[i], oDirectory | oNoFollow | oNonBlock);
                if (nextFd < 0)
                    ThrowUnixOpenError(Marshal.GetLastPInvokeError(), fullPath);

                close(directoryFd);
                directoryFd = nextFd;
            }

            var fileFd = openat(directoryFd, components[^1], oNoFollow | oNonBlock);
            if (fileFd < 0)
                ThrowUnixOpenError(Marshal.GetLastPInvokeError(), fullPath);

            SafeFileHandle? handle = new((IntPtr)fileFd, ownsHandle: true);
            try
            {
                if (!IsRegularUnixFile(fileFd))
                    throw new IOException("artifact is not a regular file");

                var stream = new FileStream(handle, FileAccess.Read);
                handle = null;
                return stream;
            }
            finally
            {
                handle?.Dispose();
            }
        }
        finally
        {
            if (directoryFd >= 0)
                close(directoryFd);
        }
    }

    static bool IsRegularUnixFile(int fd)
    {
        if (OperatingSystem.IsLinux())
        {
            const int atEmptyPath = 0x1000;
            const uint statxType = 0x0001;
            var buffer = Marshal.AllocHGlobal(256);
            try
            {
                for (var i = 0; i < 256; i++)
                    Marshal.WriteByte(buffer, i, 0);

                if (statx(fd, string.Empty, atEmptyPath, statxType, buffer) != 0)
                    throw new Win32Exception(Marshal.GetLastPInvokeError());

                var mode = unchecked((ushort)Marshal.ReadInt16(buffer, 28));
                return (mode & S_IFMT) == S_IFREG;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        if (OperatingSystem.IsMacOS())
        {
            var buffer = Marshal.AllocHGlobal(144);
            try
            {
                for (var i = 0; i < 144; i++)
                    Marshal.WriteByte(buffer, i, 0);

                if (fstat(fd, buffer) != 0)
                    throw new Win32Exception(Marshal.GetLastPInvokeError());

                var mode = unchecked((ushort)Marshal.ReadInt16(buffer, 4));
                return (mode & S_IFMT) == S_IFREG;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        throw new PlatformNotSupportedException("secure artifact verification supports Windows, Linux, and macOS");
    }

    static void ThrowUnixOpenError(int error, string path)
    {
        var eLoop = OperatingSystem.IsMacOS() ? 62 : 40;
        if (error == eLoop)
            throw new ArtifactLinkException("artifact path contains a symbolic link");

        if (error == 2)
            throw new FileNotFoundException("artifact does not exist", path);

        throw new Win32Exception(error);
    }

    [DllImport("libc", SetLastError = true)]
    static extern int open(string path, int flags);

    [DllImport("libc", SetLastError = true)]
    static extern int openat(int directoryFd, string path, int flags);

    [DllImport("libc", SetLastError = true)]
    static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    static extern int statx(int directoryFd, string path, int flags, uint mask, IntPtr buffer);

    [DllImport("libc", SetLastError = true)]
    static extern int fstat(int fd, IntPtr buffer);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern uint GetFileType(SafeFileHandle handle);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern uint GetFinalPathNameByHandle(
        SafeFileHandle file,
        [Out] StringBuilder filePath,
        uint filePathLength,
        uint flags);
}
