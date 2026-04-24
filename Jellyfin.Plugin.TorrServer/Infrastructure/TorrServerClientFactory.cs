using System;
using Jellyfin.Plugin.TorrServer.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TorrServer.Infrastructure;

internal class TorrServerClientFactory(IServiceProvider serviceProvider) : ITorrServerClientFactory
{
    public IApiClient GetClient() => serviceProvider.GetRequiredService<IApiClient>();
}