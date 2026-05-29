using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Checks whether the Playback Reporting plugin is installed and usable.
/// </summary>
public class PlaybackReportingDependencyChecker : IPlaybackReportingDependencyChecker
{
    private readonly IPluginManager _pluginManager;
    private readonly IServerApplicationPaths _applicationPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackReportingDependencyChecker"/> class.
    /// </summary>
    /// <param name="pluginManager">Plugin manager.</param>
    /// <param name="applicationPaths">Application paths.</param>
    public PlaybackReportingDependencyChecker(
        IPluginManager pluginManager,
        IServerApplicationPaths applicationPaths)
    {
        _pluginManager = pluginManager;
        _applicationPaths = applicationPaths;
    }

    /// <inheritdoc />
    public PlaybackReportingDependencyResult Check()
    {
        var plugin = _pluginManager.GetPlugin(PlaybackReportingConstants.PluginId);
        var isActive = plugin?.Manifest.Status == PluginStatus.Active && plugin?.Instance is not null;

        return PlaybackReportingDependencyEvaluator.Evaluate(
            isInstalled: plugin is not null,
            isPluginActive: isActive,
            databaseExists: DatabaseExists(),
            inactiveStatusLabel: plugin?.Manifest.Status.ToString());
    }

    private bool DatabaseExists()
    {
        var dbPath = Path.GetFullPath(Path.Combine(_applicationPaths.DataPath, PlaybackReportingConstants.DatabaseFileName));

        if (!PathSafety.IsFileInDirectory(dbPath, _applicationPaths.DataPath)
            || !string.Equals(Path.GetFileName(dbPath), PlaybackReportingConstants.DatabaseFileName, StringComparison.Ordinal))
        {
            return false;
        }

        return File.Exists(dbPath);
    }
}
