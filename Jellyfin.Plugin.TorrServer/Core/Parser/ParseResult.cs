using System.Collections.Generic;

namespace Jellyfin.Plugin.TorrServer.Core.Parser;

internal class ParseResult
{
    public string OriginalName { get; init; } = null!;

    public string Title { get; init; } = null!;

    public int? Year { get; init; }

    public IReadOnlyList<string> VersionTags { get; init; } = [];

    public IReadOnlyList<string> TechnicalTags { get; init; } = [];
}
