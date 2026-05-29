using Jellyfin.Plugin.PlaybackReportingTopX.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Validates the Playback Reporting dependency after Jellyfin startup.
/// </summary>
public class PluginStartupService : IHostedService
{
    private readonly IPlaybackReportingDependencyChecker _dependencyChecker;
    private readonly ILogger<PluginStartupService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginStartupService"/> class.
    /// </summary>
    /// <param name="dependencyChecker">Dependency checker.</param>
    /// <param name="logger">Logger.</param>
    public PluginStartupService(
        IPlaybackReportingDependencyChecker dependencyChecker,
        ILogger<PluginStartupService> logger)
    {
        _dependencyChecker = dependencyChecker;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var result = _dependencyChecker.Check();
        if (result.IsReady)
        {
            _logger.LogInformation("Playback Reporting dependency check passed.");
        }
        else
        {
            _logger.LogWarning(
                "Playback Reporting Top X loaded, but {PluginName} is not ready: {Message}",
                PlaybackReportingConstants.PluginName,
                result.Message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
