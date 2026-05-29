namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Evaluates Playback Reporting dependency state.
/// </summary>
internal static class PlaybackReportingDependencyEvaluator
{
    /// <summary>
    /// Evaluates dependency readiness from plugin and database state.
    /// </summary>
    /// <param name="isInstalled">Whether the plugin package is present.</param>
    /// <param name="isPluginActive">Whether the plugin is active and loaded.</param>
    /// <param name="databaseExists">Whether the Playback Reporting database exists.</param>
    /// <param name="inactiveStatusLabel">Optional status label when the plugin is inactive.</param>
    /// <returns>Dependency check result.</returns>
    internal static PlaybackReportingDependencyResult Evaluate(
        bool isInstalled,
        bool isPluginActive,
        bool databaseExists,
        string? inactiveStatusLabel = null)
    {
        if (!isInstalled)
        {
            return new PlaybackReportingDependencyResult(
                PlaybackReportingDependencyState.NotInstalled,
                $"{PlaybackReportingConstants.PluginName} is not installed. Install it from Dashboard → Plugins → Catalog, then restart Jellyfin.");
        }

        if (!isPluginActive)
        {
            var statusLabel = inactiveStatusLabel ?? "Unknown";
            return new PlaybackReportingDependencyResult(
                PlaybackReportingDependencyState.NotActive,
                $"{PlaybackReportingConstants.PluginName} is installed but not active (status: {statusLabel}). Enable it in Dashboard → Plugins and restart Jellyfin.");
        }

        if (!databaseExists)
        {
            return new PlaybackReportingDependencyResult(
                PlaybackReportingDependencyState.DatabaseMissing,
                $"{PlaybackReportingConstants.PluginName} is active, but no playback database was found yet. Play some media or open {PlaybackReportingConstants.PluginName} once to initialize data collection.");
        }

        return new PlaybackReportingDependencyResult(
            PlaybackReportingDependencyState.Ready,
            $"{PlaybackReportingConstants.PluginName} is installed and ready.");
    }
}
