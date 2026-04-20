using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Jellyfin.Plugin.TorrServer.Client;

[UsedImplicitly]
internal class TorrentFile
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public long Length { get; set; }
}