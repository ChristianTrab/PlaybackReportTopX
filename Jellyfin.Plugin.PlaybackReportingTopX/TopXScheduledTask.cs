using Jellyfin.Plugin.PlaybackReportingTopX.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Scheduled task that rebuilds Top X movie and series collections.
/// </summary>
public class TopXScheduledTask : IScheduledTask
{
    private readonly IPlaybackReportingDependencyChecker _dependencyChecker;
    private readonly IPlaybackReportingDataService _playbackReportingDataService;
    private readonly ITopXCollectionService _topXCollectionService;
    private readonly ILogger<TopXScheduledTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopXScheduledTask"/> class.
    /// </summary>
    /// <param name="dependencyChecker">Playback Reporting dependency checker.</param>
    /// <param name="playbackReportingDataService">Playback reporting data service.</param>
    /// <param name="topXCollectionService">Top X collection service.</param>
    /// <param name="logger">Logger.</param>
    public TopXScheduledTask(
        IPlaybackReportingDependencyChecker dependencyChecker,
        IPlaybackReportingDataService playbackReportingDataService,
        ITopXCollectionService topXCollectionService,
        ILogger<TopXScheduledTask> logger)
    {
        _dependencyChecker = dependencyChecker;
        _playbackReportingDataService = playbackReportingDataService;
        _topXCollectionService = topXCollectionService;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Update Playback Reporting Top X Collections";

    /// <inheritdoc />
    public string Key => "PlaybackReportingTopXRefreshTask";

    /// <inheritdoc />
    public string Description => "Queries Playback Reporting watch logs and rebuilds the Top X movie and series collections.";

    /// <inheritdoc />
    public string Category => "Library";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var dependency = _dependencyChecker.Check();
        if (!dependency.IsReady)
        {
            _logger.LogWarning("Skipping Top X refresh: {Message}", dependency.Message);
            progress.Report(100.0);
            return;
        }

        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var topCount = PluginSettingsValidator.NormalizeTopCount(config.TopCount);
        var days = PluginSettingsValidator.NormalizeDaysToLookBack(config.DaysToLookBack);
        var moviesCollectionName = PluginSettingsValidator.NormalizeCollectionName(
            config.MoviesCollectionName,
            "Top 10 Movies");
        var seriesCollectionName = PluginSettingsValidator.NormalizeCollectionName(
            config.SeriesCollectionName,
            "Top 10 Series");

        _logger.LogInformation(
            "Starting Top X refresh using the last {Days} days and a limit of {TopCount} items.",
            days,
            topCount);

        var stepCount = (config.UpdateMoviesCollection ? 1 : 0) + (config.UpdateSeriesCollection ? 1 : 0);
        var completedSteps = 0;

        if (config.UpdateMoviesCollection)
        {
            var topMovieIds = await _playbackReportingDataService
                .GetTopMovieIdsAsync(days, topCount, cancellationToken)
                .ConfigureAwait(false);

            await _topXCollectionService
                .SyncCollectionAsync(moviesCollectionName, topMovieIds, cancellationToken)
                .ConfigureAwait(false);

            completedSteps++;
            progress.Report(stepCount == 0 ? 100 : (completedSteps * 100.0) / stepCount);
        }

        if (config.UpdateSeriesCollection)
        {
            var topSeriesIds = await _playbackReportingDataService
                .GetTopSeriesIdsAsync(days, topCount, cancellationToken)
                .ConfigureAwait(false);

            await _topXCollectionService
                .SyncCollectionAsync(seriesCollectionName, topSeriesIds, cancellationToken)
                .ConfigureAwait(false);

            completedSteps++;
            progress.Report(stepCount == 0 ? 100 : (completedSteps * 100.0) / stepCount);
        }

        if (stepCount == 0)
        {
            _logger.LogWarning("Both movie and series collection updates are disabled in plugin configuration.");
        }

        progress.Report(100.0);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.DailyTrigger,
                TimeOfDayTicks = TimeSpan.FromHours(2).Ticks
            }
        };
    }
}
