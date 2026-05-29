namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Validates and normalizes plugin configuration values.
/// </summary>
internal static class PluginSettingsValidator
{
    /// <summary>
    /// Maximum number of items allowed in a collection.
    /// </summary>
    internal const int MaxTopCount = 100;

    /// <summary>
    /// Maximum number of days to look back in playback history.
    /// </summary>
    internal const int MaxDaysToLookBack = 3650;

    /// <summary>
    /// Maximum length for a collection name.
    /// </summary>
    internal const int MaxCollectionNameLength = 256;

    /// <summary>
    /// Maximum episode rows read when aggregating series statistics.
    /// </summary>
    internal const int MaxEpisodeScanRows = 10000;

    /// <summary>
    /// Normalizes the top count setting.
    /// </summary>
    /// <param name="value">Configured value.</param>
    /// <returns>Clamped top count.</returns>
    internal static int NormalizeTopCount(int value) => Math.Clamp(value, 1, MaxTopCount);

    /// <summary>
    /// Normalizes the days-to-look-back setting.
    /// </summary>
    /// <param name="value">Configured value.</param>
    /// <returns>Clamped day count.</returns>
    internal static int NormalizeDaysToLookBack(int value) => Math.Clamp(value, 1, MaxDaysToLookBack);

    /// <summary>
    /// Normalizes a collection name.
    /// </summary>
    /// <param name="value">Configured name.</param>
    /// <param name="fallback">Fallback when the value is invalid.</param>
    /// <returns>A safe collection name.</returns>
    internal static string NormalizeCollectionName(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxCollectionNameLength)
        {
            trimmed = trimmed[..MaxCollectionNameLength];
        }

        return trimmed;
    }

    /// <summary>
    /// Calculates the maximum episode rows to scan for series aggregation.
    /// </summary>
    /// <param name="topCount">Configured top count.</param>
    /// <returns>Episode scan limit.</returns>
    internal static int GetEpisodeScanLimit(int topCount)
        => Math.Min(Math.Max(NormalizeTopCount(topCount), 1) * 200, MaxEpisodeScanRows);
}
