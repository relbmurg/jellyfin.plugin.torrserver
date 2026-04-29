using System;
using System.Collections.Immutable;
using System.Linq;
using Jellyfin.Plugin.TorrServer.Extensions;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class RemoveReleaseGroup : IParsingStep
{
    private static ImmutableHashSet<string> ReleaseGroups { get; } = ImmutableHashSet.Create<string>(
        StringComparer.OrdinalIgnoreCase,
        "RGzsRutracker",
        "EniaHD");

    public void Parse(ParsingContext context)
    {
        if (context.Tokens.Count == 0)
        {
            return;
        }

        var last = context.Tokens[^1];

        // Обычный GROUP в конце
        if (context.Tokens.Count > 1 && IsReleaseGroup(last))
        {
            context.Tokens.RemoveAt(context.Tokens.Count - 1);
        }

        return;

        static bool IsReleaseGroup(string token)
        {
            if (ReleaseGroups.Contains(token))
            {
                return true;
            }

            if (token.Length is < 2 or > 15)
            {
                return false;
            }

            if (token.IsRomanNumber())
            {
                return false;
            }

            return token.All(ch => char.IsUpper(ch) || char.IsDigit(ch));
        }
    }
}
