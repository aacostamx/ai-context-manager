namespace AIContextManager;

internal static class PathUtil
{
    public static bool IsSameOrUnder(string path, string root)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root)) return false;
        var p = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)) + Path.DirectorySeparatorChar;
        var r = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return p.StartsWith(r, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Best-effort detection of OneDrive / SharePoint synced folders.</summary>
    public static bool IsCloudSynced(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        foreach (var variable in new[] { "OneDrive", "OneDriveCommercial", "OneDriveConsumer" })
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(root) && IsSameOrUnder(path, root)) return true;
        }

        return Path.GetFullPath(path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(s => s.StartsWith("OneDrive", StringComparison.OrdinalIgnoreCase) ||
                      s.StartsWith("SharePoint", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Relative Markdown link with forward slashes and escaped segments.</summary>
    public static string MarkdownLink(string relativePath) =>
        string.Join('/', relativePath.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString));

    public static void WriteAtomically(string file, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var tmp = file + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, file, overwrite: true);
    }

    public static void TryDeleteDirectory(string dir)
    {
        try
        {
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(dir, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
