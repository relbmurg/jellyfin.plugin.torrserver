using Jellyfin.Plugin.TorrServer.Core.Parser;

namespace Jellyfin.Plugin.TorrServer.Extensions;

internal static class ParseResultExtensions
{
    public static string FolderName(this ParseResult result) => result.Year.HasValue ? $"{result.Title} ({result.Year})" : result.Title;

    public static string StrmFileName(this ParseResult result)
    {
        var titlePart = result.Year.HasValue ? $"{result.Title} ({result.Year})" : result.Title;
        var versionPart = result.VersionTags.Count > 0 ? $" [{string.Join(" ", result.VersionTags)}]" : string.Empty;
        var techPart = result.TechnicalTags.Count > 0 ? $" [{string.Join(" ", result.TechnicalTags)}]" : string.Empty;
        return $"{titlePart}{versionPart}{techPart}.strm";
    }
}
