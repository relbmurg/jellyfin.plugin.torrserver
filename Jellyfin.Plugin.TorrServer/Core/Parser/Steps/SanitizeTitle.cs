using System.Collections.Generic;
using System.Text;

namespace Jellyfin.Plugin.TorrServer.Core.Parser.Steps;

internal class SanitizeTitle(char[] wrong, char replacement) : IParsingStep
{
    public void Parse(ParsingContext context)
    {
        context.Title = ReplaceChars(context.Title, wrong, replacement);
    }

    private static string ReplaceChars(string input, char[] charsToReplace, char replacement)
    {
        var set = new HashSet<char>(charsToReplace);
        var sb = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            sb.Append(set.Contains(c) ? replacement : c);
        }

        return sb.ToString();
    }
}