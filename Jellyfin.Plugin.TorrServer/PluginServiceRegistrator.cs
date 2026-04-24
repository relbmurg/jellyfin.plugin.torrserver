using System;
using System.Net.Http.Headers;
using System.Text;
using Jellyfin.Plugin.TorrServer.Abstractions;
using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Core;
using Jellyfin.Plugin.TorrServer.Infrastructure;
using JetBrains.Annotations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TorrServer;

/// <inheritdoc />
[UsedImplicitly]
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IPluginConfigurationProvider, PluginConfigurationProvider>();
        serviceCollection.AddSingleton<ITorrServerClientFactory, TorrServerClientFactory>();

        serviceCollection.AddHttpClient<IApiClient, ApiClient>()
            .ConfigureHttpClient((provider, client) =>
            {
                var cfg = provider.GetRequiredService<IPluginConfigurationProvider>().Get();
                client.BaseAddress = Uri.TryCreate(cfg.ServerUrl, UriKind.Absolute, out var uri)
                    ? uri
                    : new Uri(Constants.TorrServer.DefaultBaseUrl);

                if (string.IsNullOrWhiteSpace(cfg.Username))
                {
                    return;
                }

                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{cfg.Username}:{cfg.Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            });
        serviceCollection.AddSingleton<WatchdogService>();
        serviceCollection.AddSingleton<ISyncService>(services => services.GetRequiredService<WatchdogService>());
        serviceCollection.AddHostedService(services => services.GetRequiredService<WatchdogService>());
    }
}
