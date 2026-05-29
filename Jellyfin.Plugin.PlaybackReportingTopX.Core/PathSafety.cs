namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Path validation helpers.
/// </summary>
internal static class PathSafety
{
    /// <summary>
    /// Determines whether a file path is located inside the expected directory.
    /// </summary>
    /// <param name="filePath">Candidate file path.</param>
    /// <param name="directoryPath">Expected containing directory.</param>
    /// <returns>True when the file is inside the directory.</returns>
    internal static bool IsFileInDirectory(string filePath, string directoryPath)
    {
        var fullFilePath = Path.GetFullPath(filePath);
        var fullDirectoryPath = Path.GetFullPath(directoryPath);

        if (!fullDirectoryPath.EndsWith(Path.DirectorySeparatorChar))
        {
            fullDirectoryPath += Path.DirectorySeparatorChar;
        }

        return fullFilePath.StartsWith(
            fullDirectoryPath,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
