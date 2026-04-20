using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Core;
using JetBrains.Annotations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using static Jellyfin.Plugin.TorrServer.Constants.TorrServer;

namespace Jellyfin.Plugin.TorrServer;

/// <inheritdoc />
[UsedImplicitly]
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IPluginConfigurationProvider, PluginConfigurationProvider>();
        serviceCollection.AddHttpClient(HttpClientName);
        serviceCollection.AddScoped<ISyncService, SyncService>();
        serviceCollection.AddScoped<IApiClient, ApiClient>();
        serviceCollection.AddLocalization();
    }
}
