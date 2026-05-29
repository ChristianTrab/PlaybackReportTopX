using System.Globalization;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Reads playback statistics from the Playback Reporting SQLite database.
/// </summary>
public class PlaybackReportingDataService : IPlaybackReportingDataService
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IServerApplicationPaths _applicationPaths;
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<PlaybackReportingDataService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackReportingDataService"/> class.
    /// </summary>
    /// <param name="applicationPaths">Application paths.</param>
    /// <param name="libraryManager">Library manager.</param>
    /// <param name="logger">Logger.</param>
    public PlaybackReportingDataService(
        IServerApplicationPaths applicationPaths,
        ILibraryManager libraryManager,
        ILogger<PlaybackReportingDataService> logger)
    {
        _applicationPaths = applicationPaths;
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Guid>> GetTopMovieIdsAsync(int days, int limit, CancellationToken cancellationToken)
    {
        var normalizedDays = PluginSettingsValidator.NormalizeDaysToLookBack(days);
        var normalizedLimit = PluginSettingsValidator.NormalizeTopCount(limit);

        return Task.Run(
            () => QueryTopItemIds(
                itemType: "Movie",
                normalizedDays,
                normalizedLimit,
                rowLimit: normalizedLimit,
                cancellationToken,
                mapEpisodeToSeries: false),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Guid>> GetTopSeriesIdsAsync(int days, int limit, CancellationToken cancellationToken)
    {
        var normalizedDays = PluginSettingsValidator.NormalizeDaysToLookBack(days);
        var normalizedLimit = PluginSettingsValidator.NormalizeTopCount(limit);
        var rowLimit = PluginSettingsValidator.GetEpisodeScanLimit(normalizedLimit);

        return Task.Run(
            () => QueryTopItemIds(
                itemType: "Episode",
                normalizedDays,
                normalizedLimit,
                rowLimit,
                cancellationToken,
                mapEpisodeToSeries: true),
            cancellationToken);
    }

    private IReadOnlyList<Guid> QueryTopItemIds(
        string itemType,
        int days,
        int limit,
        int rowLimit,
        CancellationToken cancellationToken,
        bool mapEpisodeToSeries)
    {
        var dbPath = Path.GetFullPath(Path.Combine(_applicationPaths.DataPath, PlaybackReportingConstants.DatabaseFileName));

        if (!PathSafety.IsFileInDirectory(dbPath, _applicationPaths.DataPath)
            || !string.Equals(Path.GetFileName(dbPath), PlaybackReportingConstants.DatabaseFileName, StringComparison.Ordinal))
        {
            _logger.LogWarning("Refusing to open an unexpected Playback Reporting database path.");
            return Array.Empty<Guid>();
        }

        if (!File.Exists(dbPath))
        {
            _logger.LogWarning(
                "Playback Reporting database not found. Install and run the Playback Reporting plugin first.");
            return Array.Empty<Guid>();
        }

        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);

        const string sql = """
            SELECT ItemId, COUNT(1) AS PlayCount, SUM(PlayDuration) AS TotalDuration
            FROM PlaybackActivity
            WHERE ItemType = @itemType
              AND DateCreated >= @startDate
              AND DateCreated <= @endDate
              AND (
                NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'UserList')
                OR UserId NOT IN (SELECT UserId FROM UserList)
              )
            GROUP BY ItemId
            ORDER BY PlayCount DESC, TotalDuration DESC
            LIMIT @rowLimit
            """;

        var rawStats = new List<(Guid ItemId, int PlayCount, long TotalDuration)>();

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@itemType", itemType);
            command.Parameters.AddWithValue("@startDate", startDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@endDate", endDate.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@rowLimit", rowLimit);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Guid.TryParse(reader.GetString(0), out var itemId))
                {
                    continue;
                }

                rawStats.Add((itemId, reader.GetInt32(1), reader.GetInt64(2)));
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read playback statistics from the Playback Reporting database.");
            return Array.Empty<Guid>();
        }

        if (rawStats.Count == 0)
        {
            _logger.LogInformation("No {ItemType} playback data found for the last {Days} days.", itemType, days);
            return Array.Empty<Guid>();
        }

        if (!mapEpisodeToSeries)
        {
            return rawStats
                .Where(stat => _libraryManager.GetItemById(stat.ItemId) is not null)
                .Take(limit)
                .Select(stat => stat.ItemId)
                .ToList();
        }

        var seriesStats = new Dictionary<Guid, (int PlayCount, long TotalDuration)>();
        var unmappedEpisodeRows = 0;

        foreach (var stat in rawStats)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_libraryManager.GetItemById(stat.ItemId) is not Episode episode || episode.SeriesId == Guid.Empty)
            {
                unmappedEpisodeRows++;
                continue;
            }

            if (seriesStats.TryGetValue(episode.SeriesId, out var existing))
            {
                seriesStats[episode.SeriesId] = (
                    existing.PlayCount + stat.PlayCount,
                    existing.TotalDuration + stat.TotalDuration);
            }
            else
            {
                seriesStats[episode.SeriesId] = (stat.PlayCount, stat.TotalDuration);
            }
        }

        if (unmappedEpisodeRows > 0)
        {
            _logger.LogDebug(
                "Skipped {UnmappedRows} episode playback rows that could not be mapped to a library series.",
                unmappedEpisodeRows);
        }

        var topSeriesIds = seriesStats
            .OrderByDescending(pair => pair.Value.PlayCount)
            .ThenByDescending(pair => pair.Value.TotalDuration)
            .Take(limit)
            .Select(pair => pair.Key)
            .ToList();

        if (topSeriesIds.Count < limit)
        {
            _logger.LogInformation(
                "Found {SeriesCount} ranked series in the last {Days} days (requested top {Limit}).",
                topSeriesIds.Count,
                days,
                limit);
        }

        return topSeriesIds;
    }
}
