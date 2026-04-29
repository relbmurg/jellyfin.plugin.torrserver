using System.Collections.Generic;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal class ParsingContext(string original)
{
    public string OriginalName { get; } = original;

    public string WorkingName { get; set; } = original;

    public int? Year { get; set; }

    public List<string> VersionTags { get; } = [];

    public List<string> TechnicalTags { get; } = [];

    public List<string> Tokens { get; } = [];

    public string SeasonEpisode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
