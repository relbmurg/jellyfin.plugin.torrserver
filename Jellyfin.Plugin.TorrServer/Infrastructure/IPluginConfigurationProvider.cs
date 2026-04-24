using Jellyfin.Plugin.TorrServer.Configuration;

namespace Jellyfin.Plugin.TorrServer.Infrastructure;

internal interface IPluginConfigurationProvider
{
    public PluginConfiguration Get();
}
