namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Creates and updates Top X Jellyfin collections.
/// </summary>
public interface ITopXCollectionService
{
    /// <summary>
    /// Finds or creates a collection and syncs it with the provided item IDs.
    /// </summary>
    /// <param name="collectionName">Collection display name.</param>
    /// <param name="itemIds">Ordered item IDs to place in the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the collection was updated.</returns>
    Task<bool> SyncCollectionAsync(string collectionName, IReadOnlyList<Guid> itemIds, CancellationToken cancellationToken);
}
