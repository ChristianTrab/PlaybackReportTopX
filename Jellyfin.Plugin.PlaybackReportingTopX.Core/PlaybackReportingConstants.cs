namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Constants for the Playback Reporting plugin dependency.
/// </summary>
internal static class PlaybackReportingConstants
{
    /// <summary>
    /// Playback Reporting plugin identifier.
    /// </summary>
    internal static readonly Guid PluginId = Guid.Parse("5c534381-91a3-43cb-907a-35aa02eb9d2c");

    /// <summary>
    /// Playback Reporting plugin display name.
    /// </summary>
    internal const string PluginName = "Playback Reporting";

    /// <summary>
    /// Playback Reporting SQLite database file name.
    /// </summary>
    internal const string DatabaseFileName = "playback_reporting.db";
}
