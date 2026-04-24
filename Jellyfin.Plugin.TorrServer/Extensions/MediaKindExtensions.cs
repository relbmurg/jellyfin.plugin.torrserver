using Jellyfin.Plugin.TorrServer.Client;
using Jellyfin.Plugin.TorrServer.Core;

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static class MediaKindExtensions
{
    public static Category ToCategory(this MediaKind kind) => kind switch
    {
        MediaKind.Movie => Category.Movie,
        MediaKind.Series => Category.Tv,
        MediaKind.Season => Category.Tv,
        MediaKind.Episode => Category.Tv,
        _ => Category.Unknown
    };
}