using Jellyfin.Plugin.PlaybackReportingTopX;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Tests;

public class PathSafetyTests
{
    [Fact]
    public void IsFileInDirectory_ReturnsTrueForFileInsideDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "PlaybackReportTopXSafeDir");
        var file = Path.Combine(directory, PlaybackReportingConstants.DatabaseFileName);

        Assert.True(PathSafety.IsFileInDirectory(file, directory));
    }

    [Fact]
    public void IsFileInDirectory_ReturnsFalseForPrefixDirectoryBypass()
    {
        var directory = Path.Combine(Path.GetTempPath(), "PlaybackReportTopXSafeDir");
        var siblingDirectory = directory + "_extra";
        var file = Path.Combine(siblingDirectory, PlaybackReportingConstants.DatabaseFileName);

        Assert.False(PathSafety.IsFileInDirectory(file, directory));
    }
}
