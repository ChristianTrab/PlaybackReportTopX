namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Reads top watched items from the Playback Reporting database.
/// </summary>
public interface IPlaybackReportingDataService
{
    /// <summary>
    /// Gets the most watched movie item IDs.
    /// </summary>
    /// <param name="days">Number of days of history to include.</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of movie item IDs.</returns>
    Task<IReadOnlyList<Guid>> GetTopMovieIdsAsync(int days, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the most watched series item IDs.
    /// </summary>
    /// <param name="days">Number of days of history to include.</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of series item IDs.</returns>
    Task<IReadOnlyList<Guid>> GetTopSeriesIdsAsync(int days, int limit, CancellationToken cancellationToken);
}
