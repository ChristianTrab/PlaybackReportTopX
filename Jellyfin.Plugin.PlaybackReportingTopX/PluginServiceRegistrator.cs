using Jellyfin.Plugin.PlaybackReportingTopX.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.PlaybackReportingTopX;

/// <summary>
/// Registers plugin services with the Jellyfin DI container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IPlaybackReportingDependencyChecker, PlaybackReportingDependencyChecker>();
        serviceCollection.AddSingleton<IPlaybackReportingDataService, PlaybackReportingDataService>();
        serviceCollection.AddSingleton<ITopXCollectionService, TopXCollectionService>();
        serviceCollection.AddHostedService<PluginStartupService>();
    }
}
