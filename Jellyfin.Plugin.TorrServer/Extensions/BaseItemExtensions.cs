using System.Text.Json;
using Jellyfin.Plugin.TorrServer.Core;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static class BaseItemExtensions
{
    private const string MetadataKey = Constants.PluginId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static MediaKind GetMediaKind(this BaseItem item) => item switch
    {
        Movie => MediaKind.Movie,
        Series => MediaKind.Series,
        Season => MediaKind.Season,
        Episode => MediaKind.Episode,
        _ => MediaKind.Other
    };

    public static ItemMetadata? GetItemMetadata(this BaseItem item) =>
        item.ProviderIds.TryGetValue(MetadataKey, out var json)
            ? JsonSerializer.Deserialize<ItemMetadata>(json, JsonOptions)
            : null;

    public static void SaveItemMetadata(this BaseItem item, ItemMetadata metadata)
    {
        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        item.SetProviderId(MetadataKey, json);
    }
}