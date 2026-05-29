namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Dependency readiness states for Playback Reporting.
/// </summary>
public enum PlaybackReportingDependencyState
{
    /// <summary>
    /// Playback Reporting is installed, active, and its database is available.
    /// </summary>
    Ready,

    /// <summary>
    /// Playback Reporting is not installed.
    /// </summary>
    NotInstalled,

    /// <summary>
    /// Playback Reporting is installed but not active.
    /// </summary>
    NotActive,

    /// <summary>
    /// Playback Reporting is active but its database has not been created yet.
    /// </summary>
    DatabaseMissing,
}

/// <summary>
/// Result of a Playback Reporting dependency check.
/// </summary>
/// <param name="State">Dependency state.</param>
/// <param name="Message">Human-readable status message.</param>
public readonly record struct PlaybackReportingDependencyResult(
    PlaybackReportingDependencyState State,
    string Message)
{
    /// <summary>
    /// Gets a value indicating whether Playback Reporting is ready to use.
    /// </summary>
    public bool IsReady => State == PlaybackReportingDependencyState.Ready;
}
