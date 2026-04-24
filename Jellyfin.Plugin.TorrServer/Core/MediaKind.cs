namespace Jellyfin.Plugin.TorrServer.Core;

/// <summary>
/// Kind of media type.
/// </summary>
public enum MediaKind
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Movie,
    Series,
    Season,
    Episode,
    Other
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}