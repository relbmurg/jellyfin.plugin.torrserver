using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Jellyfin.Plugin.TorrServer.Client;

[UsedImplicitly]
internal class TorrentItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonCategoryEnumConverter))]
    public Category Category { get; set; }

    [JsonPropertyName("poster")]
    public string Poster { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public int Timestamp { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("stat")]
    public int State { get; set; }

    [JsonPropertyName("stat_string")]
    public string StateString { get; set; } = string.Empty;

    [JsonPropertyName("torrent_size")]
    public long Size { get; set; }

    [JsonPropertyName("file_stats")]
    public TorrentFile[] Files { get; set; } = [];
}