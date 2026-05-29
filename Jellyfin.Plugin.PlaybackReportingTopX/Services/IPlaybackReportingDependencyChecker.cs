namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Checks whether the Playback Reporting plugin is installed and usable.
/// </summary>
public interface IPlaybackReportingDependencyChecker
{
    /// <summary>
    /// Checks whether Playback Reporting is installed, active, and initialized.
    /// </summary>
    /// <returns>Dependency check result.</returns>
    PlaybackReportingDependencyResult Check();
}
