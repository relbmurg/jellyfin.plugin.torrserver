using System.Collections.Generic;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal class ParseResult(ParsingContext context)
{
    public string OriginalName { get; } = context.OriginalName;

    public string Title { get; } = context.Title;

    public int? Year { get; } = context.Year;

    public IReadOnlyList<string> VersionTags { get; } = context.VersionTags;

    public IReadOnlyList<string> TechnicalTags { get; } = context.TechnicalTags;

    public int? Season { get; } = context.Season;

    public int? Episode { get; } = context.Episode;

    public string SeasonEpisodeTag =>
        (Season.HasValue ? $"S{Season.Value:D2}" : string.Empty)
        + (Episode.HasValue ? $"E{Episode.Value:D2}" : string.Empty);

    public string FolderName => Year.HasValue ? $"{Title} ({Year})" : Title;

    public string StrmFileName
    {
        get
        {
            var titlePart = Year.HasValue ? $"{Title} ({Year})" : Title;
            if (!string.IsNullOrWhiteSpace(SeasonEpisodeTag))
            {
                titlePart = $"{titlePart} {SeasonEpisodeTag}";
            }

            var versionPart = VersionTags.Count > 0 ? $" [{string.Join(" ", VersionTags)}]" : string.Empty;
            var techPart = TechnicalTags.Count > 0 ? $" [{string.Join(" ", TechnicalTags)}]" : string.Empty;
            return $"{titlePart}{versionPart}{techPart}.strm";
        }
    }
}
