using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TorrServer.Client;

internal class ServerConfig
{
    [JsonPropertyName("TorrentDisconnectTimeout")]
    public int TorrentDisconnectTimeout { get; set; } = 30;
}