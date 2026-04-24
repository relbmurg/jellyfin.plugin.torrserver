using Jellyfin.Plugin.TorrServer.Configuration;

namespace Jellyfin.Plugin.TorrServer.Infrastructure;

internal class PluginConfigurationProvider : IPluginConfigurationProvider
{
    public PluginConfiguration Get() => Plugin.Instance!.Configuration;
}
