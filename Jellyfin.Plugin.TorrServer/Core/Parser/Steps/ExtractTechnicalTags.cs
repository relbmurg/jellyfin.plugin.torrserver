using System;
using System.Collections.Immutable;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class ExtractTechnicalTags : IParsingStep
{
    public static ImmutableHashSet<string> TechnicalSet { get; } = ImmutableHashSet.Create<string>(
        StringComparer.OrdinalIgnoreCase,
        "720p",
        "1080p",
        "2160p",
        "4K",
        "8K",
        "BluRay",
        "WEB-DL",
        "WEBRip",
        "HDRip",
        "BDRip",
        "DVDRip",
        "REMUX",
        "x264",
        "x265",
        "h264",
        "h265",
        "HEVC",
        "AVC",
        "DTS",
        "AAC",
        "AC3",
        "TrueHD",
        "Atmos",
        "Hybrid");

    public void Parse(ParsingContext context)
    {
        for (var i = context.Tokens.Count - 1; i >= 0; i--)
        {
            var token = context.Tokens[i];
            if (TechnicalSet.Contains(token))
            {
                context.TechnicalTags.Insert(0, token);
                context.Tokens.RemoveAt(i);
            }
        }
    }
}
