using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Plugin configuration for Top X collections.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the number of items to include in each collection.
    /// </summary>
    public int TopCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets how many days of playback history to consider.
    /// </summary>
    public int DaysToLookBack { get; set; } = 30;

    /// <summary>
    /// Gets or sets the movies collection name.
    /// </summary>
    public string MoviesCollectionName { get; set; } = "Top 10 Movies";

    /// <summary>
    /// Gets or sets the series collection name.
    /// </summary>
    public string SeriesCollectionName { get; set; } = "Top 10 Series";

    /// <summary>
    /// Gets or sets a value indicating whether movie collections are updated.
    /// </summary>
    public bool UpdateMoviesCollection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether series collections are updated.
    /// </summary>
    public bool UpdateSeriesCollection { get; set; } = true;
}
