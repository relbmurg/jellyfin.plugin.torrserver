using Jellyfin.Plugin.TorrServer.Configuration;

namespace Jellyfin.Plugin.TorrServer;

internal interface IPluginConfigurationProvider
{
    public PluginConfiguration Get();
}
