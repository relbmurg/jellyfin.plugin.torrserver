using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class ExtractVersionTags : IParsingStep
{
    private static Dictionary<string[], string> VersionPhrases { get; } = new(new StringArrayComparer())
    {
        [["directors", "cut"]] = "Director's Cut",
        [["director", "cut"]] = "Director's Cut",
        [["director's", "cut"]] = "Director's Cut",
        [["extended", "cut"]] = "Extended",
        [["extended"]] = "Extended",
        [["final", "cut"]] = "Final Cut",
        [["special", "edition"]] = "Special Edition",
        [["ultimate", "edition"]] = "Ultimate Edition",
        [["anniversary", "edition"]] = "Anniversary Edition",
        [["imax"]] = "IMAX",
        [["unrated"]] = "Unrated",
        [["theatrical"]] = "Theatrical"
    };

    public void Parse(ParsingContext context)
    {
        var i = 0;
        while (i < context.Tokens.Count)
        {
            var matched = false;

            // Максимальная длина фразы = 3
            for (var length = 3; length >= 1; length--)
            {
                if (i + length > context.Tokens.Count)
                {
                    continue;
                }

                var slice = context.Tokens
                    .Skip(i)
                    .Take(length)
                    .Select(t => t.ToLowerInvariant())
                    .ToArray();

                if (VersionPhrases.TryGetValue(slice, out var canonical))
                {
                    context.VersionTags.Add(canonical);

                    context.Tokens.RemoveRange(i, length);

                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                i++;
            }
        }
    }

    private sealed class StringArrayComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            if (x == null || y == null || x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!string.Equals(x[i], y[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                var hash = 17;
                foreach (var s in obj)
                {
                    hash = (hash * 23) + StringComparer.OrdinalIgnoreCase.GetHashCode(s);
                }

                return hash;
            }
        }
    }
}
