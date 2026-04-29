using System.Globalization;
using System.Text.RegularExpressions;
#pragma warning disable SA1601

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal partial class ExtractEpisodeTag : IParsingStep
{
    [GeneratedRegex(@"S(?<s>\d{1,2})E(?<e>\d{1,2})", RegexOptions.IgnoreCase)]
    private static partial Regex SxEFormat();

    [GeneratedRegex(@"(?<s>\d{1,2})[x,-](?<e>\d{1,2})", RegexOptions.IgnoreCase)]
    private static partial Regex XFormat();

    public void Parse(ParsingContext context)
    {
        for (var index = 0; index < context.Tokens.Count; index++)
        {
            var token = context.Tokens[index];
            // S01E02
            var m = SxEFormat().Match(token);
            if (m.Success)
            {
                context.Season = ParseResult(m, "s");
                context.Episode = ParseResult(m, "e");
                context.Tokens.RemoveAt(index);
                break;
            }

            // 1x02
            // 01-02
            m = XFormat().Match(token);
            if (m.Success)
            {
                context.Season = ParseResult(m, "s");
                context.Episode = ParseResult(m, "e");
                context.Tokens.RemoveAt(index);
                break;
            }
        }
    }

    private static int? ParseResult(Match match, string group)
    {
        return int.TryParse(match.Groups[group].Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo,  out var result) ? result : null;
    }
}