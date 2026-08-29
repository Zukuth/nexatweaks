using System.Security.Cryptography.X509Certificates;

namespace NexaTweaks.Core.Diagnostics;

/// <summary>Checks whether an executable is digitally signed by Microsoft - the same "is this Windows itself or something else" signal Autoruns uses to grey out entries.</summary>
public static class FileSignatureChecker
{
    public static bool IsMicrosoftSigned(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

        try
        {
            using var cert = X509Certificate.CreateFromSignedFile(filePath);
            using var cert2 = new X509Certificate2(cert);
            return cert2.Subject.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Pulls the leading executable path out of a command line that may be quoted and/or carry arguments.</summary>
    public static string? ExtractExecutablePath(string? commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return null;
        var trimmed = commandLine.Trim();

        if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            return end > 0 ? trimmed[1..end] : trimmed.Trim('"');
        }

        var spaceIdx = trimmed.IndexOf(' ');
        return spaceIdx > 0 ? trimmed[..spaceIdx] : trimmed;
    }
}
