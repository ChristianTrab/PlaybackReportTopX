using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Services;

/// <summary>
/// Creates and synchronizes Top X collections in the Jellyfin library.
/// </summary>
public class TopXCollectionService : ITopXCollectionService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ICollectionManager _collectionManager;
    private readonly ILogger<TopXCollectionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopXCollectionService"/> class.
    /// </summary>
    /// <param name="libraryManager">Library manager.</param>
    /// <param name="collectionManager">Collection manager.</param>
    /// <param name="logger">Logger.</param>
    public TopXCollectionService(
        ILibraryManager libraryManager,
        ICollectionManager collectionManager,
        ILogger<TopXCollectionService> logger)
    {
        _libraryManager = libraryManager;
        _collectionManager = collectionManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SyncCollectionAsync(string collectionName, IReadOnlyList<Guid> itemIds, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        collectionName = PluginSettingsValidator.NormalizeCollectionName(collectionName, "Top X Collection");

        if (itemIds.Count == 0)
        {
            _logger.LogInformation("No items found for collection {CollectionName}; skipping update.", collectionName);
            return false;
        }

        var collection = await FindCollectionAsync(collectionName).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (collection is null)
        {
            collection = await _collectionManager.CreateCollectionAsync(new CollectionCreationOptions
            {
                Name = collectionName,
                IsLocked = false
            }).ConfigureAwait(false);

            _logger.LogInformation("Created collection {CollectionName}.", collectionName);
        }

        var validItemIds = itemIds
            .Where(id => _libraryManager.GetItemById(id) is not null)
            .Distinct()
            .ToList();

        if (validItemIds.Count == 0)
        {
            _logger.LogWarning("No valid library items found for collection {CollectionName}.", collectionName);
            return false;
        }

        var existingChildIds = collection.LinkedChildren
            .Where(link => link.ItemId.HasValue)
            .Select(link => link.ItemId!.Value)
            .ToList();

        if (existingChildIds.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _collectionManager.RemoveFromCollectionAsync(collection.Id, existingChildIds).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _collectionManager.AddToCollectionAsync(collection.Id, validItemIds).ConfigureAwait(false);

        _logger.LogInformation(
            "Updated collection {CollectionName} with {ItemCount} items.",
            collectionName,
            validItemIds.Count);

        return true;
    }

    private async Task<BoxSet?> FindCollectionAsync(string collectionName)
    {
        var collectionsFolder = await _collectionManager.GetCollectionsFolder(false).ConfigureAwait(false);
        if (collectionsFolder is null)
        {
            return null;
        }

        return collectionsFolder.GetChildren(null, true)
            .OfType<BoxSet>()
            .FirstOrDefault(collection => string.Equals(collection.Name, collectionName, StringComparison.OrdinalIgnoreCase));
    }
}
